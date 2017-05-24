using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;
using System.Reflection;


namespace RedMoon {


public class InvalidFileException : Exception
{
    public InvalidFileException(string message)
        : base(message)
    {
    }

    public InvalidFileException()
        : this("Wrong file type or bad header")
    {
    }
}


/// <summary>
/// Used to map properties in a subclass of <seealso cref="TextLine" />
/// to their locations in a line of a <seealso cref="TextFile" />.
/// </summary>
public class TextFieldAttribute : Attribute
{
    public int Ordinal { get; set; }

    public TextFieldAttribute(int ordinal)
    {
        Ordinal = ordinal;
    }
}


/// <summary>
/// Base class representing a line in a <seealso cref="TextFile" />.
/// </summary>
public abstract class TextLine
{
    TextFieldAttribute Attr(PropertyInfo pi)
    {
        var attrs = pi.GetCustomAttributes(typeof(TextFieldAttribute), true);

        return attrs.Length == 0 ? null : (TextFieldAttribute)attrs[0];
    }

    protected IEnumerable<PropertyInfo> Properties
    {
        get
        {
            foreach (var pi in this.GetType().GetProperties(
                        BindingFlags.NonPublic | BindingFlags.Public |
                        BindingFlags.Instance))
                if (Attr(pi) != null)
                    yield return pi;
        }
    }

    protected virtual List<string> SplitLine(string raw)
    {
        return new List<string>(raw.Split(ENV.SEP_ARGS, STR.SPL_REMOVE));
    }

    public virtual void Load(string raw)
    {
        LoadValues(SplitLine(raw));
    }

    protected virtual List<string> LoadValues(List<string> values)
    {
        foreach (var pi in Properties)
            pi.SetValue(this, LoadField(pi, values[Attr(pi).Ordinal]), null);

        return values;
    }

    protected virtual object LoadField(PropertyInfo pi, string src)
    {
        var t = pi.PropertyType;

        if (t == typeof(string))
            return src;

        if (t == typeof(AIPattern))
            return (AIPattern)int.Parse(src);

        if (t == typeof(uint))
            return uint.Parse(src);

        throw new Exception("Can't load type " + t.Name);
    }

    public virtual string Save()
    {
        return string.Join(ENV.SEP, SaveValues(new List<string>()));
    }

    protected virtual List<string> SaveValues(List<string> values)
    {
        foreach (var pi in Properties)
        {
            int ord = Attr(pi).Ordinal;

            while (ord >= values.Count)
                values.Add("");

            values[ord] = SaveField(pi);
        }

        return values;
    }

    protected virtual string SaveField(PropertyInfo pi)
    {
        var t = pi.PropertyType;
        var v = pi.GetValue(this, null);

        if (t == typeof(string))
            return v is string ? (string)v : "";

        if (t == typeof(AIPattern))
            return ((int)v).ToString();

        if (t == typeof(uint))
            return v.ToString();

        throw new Exception("Can't save type " + t.Name);
    }
}


/// <summary>
/// Represents a blank line of whitespace in a <seealso cref="TextFile" />.
/// </summary>
public sealed class BlankLine : TextLine {}


/// <summary>
/// Represents a comment line (starts with a semi-colon) in a
/// <seealso cref="TextFile" />.
/// </summary>
public sealed class CommentLine : TextLine
{
    public string Comment { get; set; }

    public CommentLine(string comment = null)
    {
        Comment = comment;
    }

    public override void Load(string raw)
    {
        var spl = new List<string>(raw.Split(';'));
        spl.RemoveAt(0);
        Comment = string.Join(";", spl);
    }

    public override string Save()
    {
        return ";" + Comment;
    }
}


/// <summary>
/// Represents a Redmoon text config file, like Mop00###.rsm and MopInfo.rsm.
/// These files consist of a number of lines, each with a number of values
/// separated by whitespace. There are also blank lines and comments which
/// are both ignored.
/// </summary>
public abstract class TextFile<TLine>
    where TLine : TextLine, new()
{
    public List<TextLine> AllItems { get; set; }

    public void Load(string file_contents)
    {
        AllItems = new List<TextLine>();

        foreach (var line in file_contents.Split(ENV.SEP_LINES, STR.SPL_NONE))
        {
            TextLine n = null;

            var trimmed = line.Trim();

            if (trimmed == "")
                n = new BlankLine();
            else if (trimmed.StartsWith(";"))
                n = new CommentLine();
            else
                n = new TLine();

            n.Load(line);

            AllItems.Add(n);
        }
    }

    public string Save()
    {
        var ret = new StringBuilder();

        foreach (var item in AllItems)
        {
            if (item != AllItems[0])
                ret.Append(ENV.NL);

            ret.Append(item.Save());
        }

        return ret.ToString();
    }

    public TLine[] Items
    {
        get
        {
            var ret = new List<TLine>();

            foreach (var item in AllItems)
                if (item is TLine)
                    ret.Add((TLine)item);

            return ret.ToArray();
        }
    }
}


/// <summary>
/// Base class for Redmoon binary config files.
/// </summary>
public abstract class RedmoonBinaryFile
{
    public abstract string VERSION_STRING { get; }

    public RedmoonBinaryFile(ByteReader br)
    {
        Load(br);
    }

    protected virtual void Load(ByteReader br)
    {
        string version = br.STRING();

        if (version != VERSION_STRING)
            throw new Exception("Expected version [" + VERSION_STRING + "] " +
                                "but found [" + version + "].");
    }

    public virtual ByteWriter Save(ByteWriter bw)
    {
        bw.STRING(VERSION_STRING);

        return bw;
    }
}


/// <summary>
/// Shortcuts for reading binary data from a byte array in the format
/// used by Redmoon binary config files.
/// </summary>
public class ByteReader
{
    public Encoding Encoding { get; set; }

    public byte[] Raw { get; set; }
    public int Offset { get; set; }

    public ByteReader(byte[] raw, Encoding encoding = null)
    {
        this.Encoding = encoding ?? ENC.DEFAULT;
        this.Raw = raw;
    }

    public void Skip(int bytes)
    {
        Offset += bytes;
    }

    public void Reset()
    {
        Offset = 0;
    }

    public string STRING(Encoding encoding = null)
    {
        int length = BYTE();

        if (length == 255)
            length = USHORT();

        return (encoding ?? Encoding).GetString(BYTES(length));
    }

    /// <summary>
    /// Reads a null-terminated string from the source stream. If max_length is
    /// specified, throws an exception if no null character has been
    /// found after max_length bytes. (kind of like a timeout in networking)
    /// </summary>
    public string NTSTRING(int max_length = Int32.MaxValue,
                           Encoding encoding = null)
    {
        var data = new List<byte>();

        for (int i = 0; i < max_length; i++)
        {
            var b = BYTE();

            if (b == 0)
                return (encoding ?? Encoding).GetString(data.ToArray());

            data.Add(b);
        }

        throw new Exception("max_length " + max_length.ToString() + " " +
                            "reached before finding a null character.");
    }

    public byte[] BYTES(int bytes)
    {
        if (Offset + bytes > Raw.Length)
            throw new Exception(string.Format(
                "Attempt to read {0} bytes when {1} are left in the buffer.",
                Offset + bytes,
                Raw.Length - Offset));

        byte[] data = new byte[bytes];
        Array.Copy(Raw, Offset, data, 0, bytes);

        Offset += bytes;

        return data;
    }

    public byte[] ENDIAN(byte[] bytes)
    {
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        return bytes;
    }

    public uint UINT() {
        return BitConverter.ToUInt32(ENDIAN(BYTES(4)), 0); }

    public int INT() {
        return BitConverter.ToInt32(ENDIAN(BYTES(4)), 0); }

    public ushort USHORT() {
        return BitConverter.ToUInt16(ENDIAN(BYTES(2)), 0); }

    public short SHORT() {
        return BitConverter.ToInt16(ENDIAN(BYTES(2)), 0); }

    public byte BYTE() { return BYTES(1)[0]; }
}


/// <summary>
/// Shortcuts for writing binary data to a memory stream in the format and
/// style of Redmoon binary config files.
/// </summary>
public class ByteWriter
{
    public Encoding Encoding { get; set; }

    protected MemoryStream Raw { get; set; }

    public ByteWriter(Encoding encoding = null)
    {
        Encoding = encoding ?? ENC.DEFAULT;
        Raw = new MemoryStream();
    }

    public byte[] ENDIAN(byte[] bytes)
    {
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        return bytes;
    }

    public void STRING(string val, Encoding encoding = null)
    {
        var bytes = (encoding ?? Encoding).GetBytes(val);

        if (bytes.Length < 255)
        {
            BYTE((byte)bytes.Length);
        }
        else
        {
            BYTE(255);
            USHORT((ushort)bytes.Length);
        }

        Raw.Write(bytes, 0, bytes.Length);
    }

    public void UINT(uint val)
        { Raw.Write(ENDIAN(BitConverter.GetBytes(val)), 0, 4); }

    public void INT(int val)
        { Raw.Write(ENDIAN(BitConverter.GetBytes(val)), 0, 4); }

    public void USHORT(ushort val)
        { Raw.Write(ENDIAN(BitConverter.GetBytes(val)), 0, 2); }

    public void SHORT(short val)
        { Raw.Write(ENDIAN(BitConverter.GetBytes(val)), 0, 2); }

    public void BYTE(byte val) { Raw.Write(new byte[] { val }, 0, 1); }

    public byte[] ToByteArray() { return Raw.ToArray(); }
}


// Hashing shortcuts
public static class HASH
{
    public static string SHA256(Stream stream)
    {
        return BitConverter.ToString(new SHA256Managed().ComputeHash(stream))
               .Replace("-", String.Empty);
    }
}


// Encoding related shortcuts
public static class ENC
{
    public static Encoding DEFAULT { get { return Encoding.ASCII; } }
    public static Encoding ASCII { get { return Encoding.ASCII; } }

    public static Encoding BIG5 { get {
        return Encoding.GetEncoding("big5"); } }

    public static Encoding KR { get {
        return Encoding.GetEncoding("euc-kr"); } }
}


// String shortcuts
public static class STR
{
    public static readonly
    StringSplitOptions SPL_NONE = StringSplitOptions.None;

    public static readonly
    StringSplitOptions SPL_REMOVE = StringSplitOptions.RemoveEmptyEntries;
}


// Environment related settings/constants/etc
public static class ENV
{
    public static string NL { get { return "\r\n"; } }
    public static string SEP { get { return " "; } }

    public static readonly string[] SEP_LINES = { "\r\n", "\n", "\r" };
    public static readonly string[] SEP_ARGS = { "\t", " " };
}


}
