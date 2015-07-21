using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;



namespace RM.v38 {


public class RLE : RLEFileBase<RLE.Resource>
{
    public class Resource : RLEResourceBase { }
}


}



namespace RM {


public abstract class RLEResourceBase
{
    public uint OffsetX { get; set; }
    public uint OffsetY { get; set; }

    public Image Image { get; set; }

    public uint Unknown0 { get; set; }
    public uint Unknown1 { get; set; }
    public uint Unknown2 { get; set; }
    public uint Unknown3 { get; set; }
}


public abstract class RLEFileBase<T>
    where T : RLEResourceBase, new()
{
    public List<T> Resources { get; protected set; }

    List<object> unknowns = new List<object>();

    public RLEFileBase()
    {
        Resources = new List<T>();
    }

    protected Color[,] Palette 
    {
        get 
        {
            if (palette == null)
            {
                palette = new Color[256,256];
                for (int w = 0; w < 256; w++)
                    for (int v = 0; v < 256; v++)
                    {
                        int tmpr = (v / 2) * 2;
                        int tmpg = (v % 8) * 32;
                        int tmpb = (w * 8) + (w % 8);
                        while (tmpb > 255)
                        {
                            tmpg += 4;
                            tmpb -= 256;
                        }
                        tmpg = tmpg + (tmpg % 2);
                        while (tmpg > 255) 
                        {
                            tmpr += 2;
                            tmpg -= 256;
                        }
                        tmpr = tmpr + (tmpr % 2);
                        while (tmpr > 255)
                        {
                            tmpr -= 256;
                        }
                        palette[w,v] = Color.FromArgb(255, tmpr, tmpg, tmpb);
                    }
            }

            return palette;
        } 
    }
    Color[,] palette;

    public void Load(ByteReader br, string __filename)
    {
        if (br.NTSTRING(14, ENC.ASCII) != "Resource File")
            throw new Exception("Invalid RLE file - header mismatch.");

        unknowns.Clear();
        unknowns.Add(br.UINT()); // 4 unknown bytes

        uint total_resources = br.UINT();
        Console.WriteLine("RES: " + total_resources);
        var res_offsets = new List<uint>();
        for (uint res_index = 0; res_index < total_resources; res_index++)
        {
            var next_offset = br.UINT();

            if (next_offset == 0)
                continue;

            res_offsets.Add(next_offset);
            Console.WriteLine("OFFSET: " + res_offsets[res_offsets.Count-1]);
        }

        Resources = new List<T>();
        for (int res_i = 0; res_i < res_offsets.Count; res_i++)
        {
            uint res_offset = res_offsets[res_i];

            if (res_offset != br.Offset)
                throw new Exception(string.Format(
                    "Expected resource to start at {0} but it was {1}.",
                    res_offset, br.Offset));

            Console.WriteLine("res " + res_i + " start " + br.Offset);

            T res = null;
            try
            {
    			res = LoadResource(br);
            }
            catch (Exception ex)
            {
                File.AppendAllText("ex.txt", 
                    "EX in " + __filename + ": " + ex.Message + "\n");
            }
            Resources.Add(res);

            if (res_i < res_offsets.Count - 1)
            {
                uint next_res_offset = res_offsets[res_i + 1];
                if (br.Offset != next_res_offset)
                    Console.WriteLine("Stopped reading resource at offset " +
                                      br.Offset + " when expected " + 
                                      next_res_offset);
                br.Offset = (int)next_res_offset;
            }
        }
        Console.WriteLine("resources: " + Resources.Count);
    }
    
 	protected T LoadResource(ByteReader br)
 	{
        uint res_offset = (uint)br.Offset;

 		var res = new T();

        // Length of the resource in bytes minus 4
        uint res_len = br.UINT();

        res.OffsetX = br.UINT();
        res.OffsetY = br.UINT();

        int width = br.INT();
        int height = br.INT();
        Console.WriteLine("size: " + width.ToString() + ", " + height.ToString());

        // 16 unknown bytes
        res.Unknown0 = br.UINT();
        res.Unknown1 = br.UINT();
        res.Unknown2 = br.UINT();
        res.Unknown3 = br.UINT();

/*
        // No idea what this is, seems to be some kind of blank
        if (res.OffsetX == 99999)
        {
            br.BYTE(); // extra byte here, not sure what it is
            Resources.Add(res);
            continue;
        }
*/
        var img = new Color[width * height];

        int pixels = 0;

        int x = 0;
        int y = 0;
        while (true)
        {
            //Console.WriteLine("X: {0}, Y: {1}", x, y);
            if (x < 0) Console.ReadKey();

            if (y >= height)
            {
                Console.WriteLine("y exceeded height?");
                break;
            }

            bool next_row = false;
            while (!next_row)
            {
                /*while (x >= width)
                {
                    x -= width;
                    y++;
                }*/

                //Console.WriteLine("y: " + y.ToString());
                byte entry_type = br.BYTE();
                //Console.WriteLine("entry_type: " + entry_type.ToString());
                switch (entry_type)
                {
                case 1:
                    pixels = br.INT();
                    //Console.WriteLine("reading " + pixels.ToString());
                    //Console.WriteLine("sz: " + width + ", " + height);
                    for (int p = 0; p < pixels; p++)
                    {
                        byte left = br.BYTE();
                        byte right = br.BYTE();
                        //Console.WriteLine("{0},{1}",x,y);
                        /*while (x >= width)
                        {
                            x -= width;
                            y++;
                        }*/
                        if (y >= height)
                        {
                            Console.WriteLine("y exceeded height?");
                            goto END_OF_IMAGE;
                        }
                        img[y*width+x] = Palette[left,right];
                        x++;
                    }
                    break;
                case 2:
                    pixels = br.INT();
                    //Console.WriteLine("skipping " + pixels.ToString());
                    x += pixels / 2;
                    //x += pixels;
                    break;
                case 3:
                    byte eof = br.BYTE();
                    //Console.WriteLine("eof: " + eof.ToString());
                    if (eof == 0)
                    {
                        Console.WriteLine("end of image at offset " + br.Offset);
                        goto END_OF_IMAGE;
                    }
                    if (eof == 3)
                    {
                        x = 0;
                        y++;
                        break;
                    }
                    if (eof == 1)
                    {
                        Console.WriteLine("eof 1 at " + br.Offset);
                        //br.Offset--;
                        //Console.ReadKey();
                    }
                    else if (eof != 2)
                        throw new Exception("bad eof " + eof);
                    int ch = br.INT();
                    //Console.WriteLine("eof skip " + ch + " at " + br.Offset);
                    x += ch/2;
                    y++;
                    break;
                default:
                    throw new Exception(string.Format(
                        "Unrecognized entry_type {0} at offset {1}).",
                        entry_type, br.Offset));
                }
            }
        }

END_OF_IMAGE:
        
        var bmp = new Bitmap(width, height);
        for (int y2 = 0; y2 < height; y2++)
            for (int x2 = 0; x2 < width; x2++)
                bmp.SetPixel(x2, y2, img[y2*width+x2]);
        res.Image = bmp;
        
        //res.Image.Save("img/test" + Resources.Count.ToString()
          //             .PadLeft(2,'0') + ".bmp");
        // Check against expected end of resource
        if (res_offset + 4 + res_len != br.Offset)
            throw new Exception(string.Format(
                "Expected resource to end at {0} but it was {1}.",
                res_offset + res_len, br.Offset));
/*
        var bmp = new Bitmap(width, height);
        var bmpdata = bmp.LockBits(new Rectangle(0, 0, width, height),
                                   ImageLockMode.ReadWrite,
                                   bmp.Pixelformat);
        */
        return res;
 	}
    
    
}



}
