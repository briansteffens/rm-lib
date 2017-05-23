using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace RM.v38
{
    public class ImageDataException : Exception
    {
        public ImageDataException(string message)
            : base(message)
        {
        }
    }

    public class WeirdSizeException : Exception
    {
    }

    public class Sprite
    {
        public uint OffsetX { get; set; }
        public uint OffsetY { get; set; }

        public Image Image { get; set; }
    }

    public class SpriteFile
    {
        public string Filename { get; protected set; }
        public List<Sprite> Sprites { get; protected set; }

        public SpriteFile()
        {
            Sprites = new List<Sprite>();
        }

        static Color[,] palette;
        protected Color[,] Palette
        {
            get
            {
                if (palette != null)
                {
                    return palette;
                }

                palette = new Color[256, 256];

                for (int w = 0; w < 256; w++)
                {
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

        public void Load(string filename)
        {
            Filename = filename;
            var br = new ByteReader(File.ReadAllBytes(Filename));

            if (br.NTSTRING(14, ENC.ASCII) != "Resource File")
            {
                throw new InvalidFileException();
            }

            // 4 unknown bytes
            br.UINT();

            uint count = br.UINT();
            var offsets = new List<uint>();

            for (uint i = 0; i < count; i++)
            {
                var next_offset = br.UINT();

                if (next_offset != 0)
                {
                    offsets.Add(next_offset);
                }
            }

            Sprites = new List<Sprite>();

            for (int i = 0; i < offsets.Count; i++)
            {
                uint offset = offsets[i];

                if (offset != br.Offset)
                {
                    throw new ImageDataException(string.Format(
                        "Expected resource to start at {0} but it was {1}.",
                        offset, br.Offset));
                }

                try
                {
                    Sprites.Add(LoadSprite(br, i));
                }
                catch (WeirdSizeException)
                {
                    Debug.WriteLine("Skipping weird 99999x99999 image");
                }

                if (i < offsets.Count - 1)
                {
                    uint next_offset = offsets[i + 1];

                    if (br.Offset != next_offset)
                    {
                        Debug.WriteLine("Stopped reading resource at offset " +
                                "{0} when expected {1}", br.Offset,
                                next_offset);
                    }

                    br.Offset = (int)next_offset;
                }
            }
        }

        protected Sprite LoadSprite(ByteReader br, int spriteIndex)
        {
            Debug.WriteLine("LoadSprite() {0}:{1} at offset {2}",
                    Filename, spriteIndex, br.Offset);

            uint offset = (uint)br.Offset;

            var res = new Sprite();

            // Expected end of the resource
            uint endOffset = br.UINT() + (uint)br.Offset;

            res.OffsetX = br.UINT();
            res.OffsetY = br.UINT();

            int width = br.INT();
            int height = br.INT();

            // 16 unknown bytes
            br.UINT();
            br.UINT();
            br.UINT();
            br.UINT();

            // No idea what this is, seems to be some kind of blank
            if (res.OffsetX == 99999)
            {
                // Extra byte here, not sure what it is
                br.BYTE();
                throw new WeirdSizeException();
            }

            var bmp = new Bitmap(width, height);

            int x = 0;
            int y = 0;

            while (true)
            {
                switch (br.BYTE())
                {
                case 1:
                    int pixels = br.INT();

                    for (int p = 0; p < pixels; p++)
                    {
                        byte left = br.BYTE();
                        byte right = br.BYTE();

                        bmp.SetPixel(x, y, Palette[left, right]);
                        x++;
                    }

                    break;

                // Skip pixels left or right
                case 2:
                    x += br.INT() / 2;
                    break;

                case 3:
                    byte eof = br.BYTE();

                    switch (eof)
                    {
                    case 0:
                        goto endOfImage;

                    // This doesn't seem to do anything, rewind the reader
                    case 1:
                        br.Offset--;
                        break;

                    // Move to the next line and skip pixels left or right
                    case 2:
                        x += br.INT() / 2;
                        y++;

                        break;

                    // Move to the next line but leave x where it is
                    case 3:
                        y++;
                        break;

                    default:
                        throw new ImageDataException("Bad eof " + eof);
                    }

                    break;

                default:
                    throw new ImageDataException(string.Format(
                        "Unrecognized entry_type at offset {0}.",
                        br.Offset));
                }
            }

    endOfImage:
            res.Image = bmp;

            // Check against expected end of resource
            if (endOffset != br.Offset)
            {
                throw new ImageDataException(string.Format(
                    "Expected resource to end at {0} but it was {1}.",
                    endOffset, br.Offset));
            }

            return res;
        }
    }
}
