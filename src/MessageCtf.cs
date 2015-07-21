using System;
using System.IO;
using System.Collections.Generic;



namespace RM {



public class CTF {


public struct Ref
{
    public uint CategoryID { get; private set; }
    public uint SubCategoryID { get; private set; }
    public uint ID { get; private set; }

    public Ref(uint category, uint subcategory, uint id) 
        : this()
    {
        CategoryID = category;
        SubCategoryID = subcategory;
        ID = id;
    }

    public override string ToString()
    {
        return string.Format("{0}.{1}.{2}", 
                             CategoryID, SubCategoryID, ID);
    }

    public override int GetHashCode()
    {
        return this.ToString().GetHashCode();
    }

    public override bool Equals(object other)
    {
        if (other == null || !(other is Ref))
            return false;

        return this.ToString() == other.ToString();
    }

    public static bool operator ==(Ref left, Ref right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Ref left, Ref right)
    {
        return !left.Equals(right);
    }
}


public class Message
{
    public SubCategory SubCategory { get; set; }
    public uint ID { get; set; }
    public string Text { get; set; }

    public Ref Ref
    {
        get
        {
            return new Ref(SubCategory.Category.ID, SubCategory.ID, ID);
        }
    }
}


public sealed class SubCategory
{
    public Category Category { get; private set; }
    public string Name { get; private set; }

    public SubCategory(Category category, string name)
    {
        Category = category;
        Name = name;
    }

    public uint ID
    {
        get
        {
            return (uint)Category.IndexOf(this);
        }
    }

    public override string ToString()
    {
        return Name;
    }
}


public sealed class Category : List<SubCategory>
{
    public File File { get; private set; }
    public string Name { get; private set; }

    public Category(File file, string name)
    {
        File = file;
        Name = name;
    }

    public uint ID
    {
        get
        {
            return (uint)File.Categories.IndexOf(this);
        }
    }

    public override string ToString()
    {
        return Name;
    }
}


public class File
{
    const string VERSION_STRING = "J.C.TypeConvTool Ver 1.1.2";

    public List<Message> Messages { get; protected set; }
    public List<Category> Categories { get; protected set; }

    public Message GetMessage(uint category, uint subcategory, uint id)
    {
        return GetMessage(new Ref(category, subcategory, id));
    }

    public Message GetMessage(Ref message_ref)
    {
        var ret = Messages.Find(p => p.Ref == message_ref);

        if (ret == null)
            throw new Exception("Message.ctf entry not found: " + message_ref);

        return ret;
    }

    public SubCategory GetSubCategory(Ref ctf_ref)
    {
        return Categories[(int)ctf_ref.CategoryID][(int)ctf_ref.SubCategoryID];
    }

    public File(ByteReader br)
    {
        // Version check
        string version = br.STRING();
        if (version != VERSION_STRING)
            throw new Exception("File type/version check failed. " +
                                "Expected: [" + VERSION_STRING + "], " +
                                "Actual: [" + version + "].");

        // Load messages
        Messages = new List<Message>();

        // Hack here to avoid reading the file twice. Categories come
        // after messages in the file but messages need references to
        // categories. So save up the details for each message and lookup the
        // subcategories after loading the whole file.
        var temp_refs = new Dictionary<Message, Ref>();

        uint total_messages = br.UINT();

        for (int i = 0; i < total_messages; i++)
        {
            var msg = new Message();

            // Weird string representation of the next 3 uints. No idea why.
            br.STRING();

            var ctf_ref = new Ref(br.UINT(), br.UINT(), br.UINT());

            msg.Text = br.STRING();
            msg.ID = ctf_ref.ID;

            Messages.Add(msg);

            temp_refs.Add(msg, ctf_ref);
        }

        // Load categories
        Categories = new List<Category>();

        uint total_categories = br.UINT() + 1;
        for (uint i = 0; i < total_categories; i++)
        {
            var category = new Category(this, br.STRING().Split('_')[1]);

            uint subcats = br.UINT() + 1;
            for (uint j = 0; j < subcats; j++)
            {
                var subcat_name = br.STRING().Split('_')[1];
                category.Add(new SubCategory(category, subcat_name));
            }

            Categories.Add(category);
        }

        // Finish up hack from above: resolve category references
        foreach (var message in temp_refs.Keys)
        {
            message.SubCategory = GetSubCategory(temp_refs[message]);
        }
    }

    public ByteWriter Save(ByteWriter bw)
    {
        bw.STRING(VERSION_STRING);

        bw.UINT((uint)Messages.Count);
        foreach (var msg in Messages)
        {
            bw.STRING(string.Format("{0}\t{1}\t{2}",
                      msg.Ref.CategoryID, 
                      msg.Ref.SubCategoryID, 
                      msg.Ref.ID));

            bw.UINT(msg.Ref.CategoryID);
            bw.UINT(msg.Ref.SubCategoryID);
            bw.UINT(msg.Ref.ID);

            bw.STRING(msg.Text);
        }

        bw.UINT((uint)Categories.Count - 1);
        for (int i = 0; i < Categories.Count; i++)
        {
            var category = Categories[i];

            bw.STRING(i.ToString() + "_" + category.Name);

            bw.UINT((uint)category.Count - 1);
            for (int j = 0; j < category.Count; j++)
                bw.STRING(j.ToString() + "_" + category[j]);
        }

        return bw;
    }
}



}



}
