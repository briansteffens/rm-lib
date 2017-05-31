using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace RedMoon.v38
{
    class WeirdSizeException : Exception
    {
    }

    enum EntryType
    {
        End      = 0,
        Paint    = 1,
        MoveX    = 2,
        NextLine = 3
    }

    public class Sprite
    {
        public uint OffsetX { get; set; }
        public uint OffsetY { get; set; }

        public Image Image { get; set; }
    }

    // Represents a *.rle file with image data
    public class SpriteFile
    {
        public string Filename { get; protected set; }
        public List<Sprite> Sprites { get; protected set; }

        public SpriteFile()
        {
            Sprites = new List<Sprite>();
        }

        public SpriteFile(string filename)
        {
            Load(filename);
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

            // Load the image offsets
            uint count = br.UINT();
            var offsets = new List<uint>();

            for (uint i = 0; i < count; i++)
            {
                var offset = br.UINT();

                if (offset != 0)
                {
                    offsets.Add(offset);
                }
            }

            // Load the image at each offset
            Sprites = new List<Sprite>();

            for (int i = 0; i < offsets.Count; i++)
            {
                uint offset = offsets[i];

                try
                {
                    Sprites.Add(LoadSprite(br, i));
                }
                catch (WeirdSizeException)
                {
                    Debug.WriteLine("Skipping weird 99999x99999 image");
                }
            }
        }

        protected Sprite LoadSprite(ByteReader br, int spriteIndex)
        {
            Debug.WriteLine("LoadSprite() {0}:{1} at offset {2}",
                    Filename, spriteIndex, br.Offset);

            var ret = new Sprite();

            // Read length of resource
            br.UINT();

            ret.OffsetX = br.UINT();
            ret.OffsetY = br.UINT();

            int width = br.INT();
            int height = br.INT();

            // 16 unknown bytes
            br.UINT();
            br.UINT();
            br.UINT();
            br.UINT();

            // No idea what this is, seems to be some kind of blank
            if (ret.OffsetX == 99999)
            {
                // Extra byte here, not sure what it is
                br.BYTE();
                throw new WeirdSizeException();
            }

            var bmp = new Bitmap(width, height);
            ret.Image = bmp;

            int x = 0;
            int y = 0;

            while (true)
            {
                switch ((EntryType)br.BYTE())
                {
                case EntryType.Paint:
                    int pixels = br.INT();

                    for (int p = 0; p < pixels; p++)
                    {
                        var data = br.USHORT();

                        var b = (byte)((data & 0x1F) / 31.0f * 255);
                        var g = (byte)((data >> 5 & 0x3F) / 63.0f * 255);
                        var r = (byte)((data >> 11 & 0x1F) / 31.0f * 255);

                        bmp.SetPixel(x, y, Color.FromArgb(255, r, g, b));
                        x++;
                    }

                    break;

                case EntryType.MoveX:
                    x += br.INT() / 2;
                    break;

                case EntryType.NextLine:
                    y++;
                    break;

                case EntryType.End:
                    return ret;

                default:
                    throw new InvalidFileException(string.Format(
                        "Unrecognized entry at offset {0}.", br.Offset));
                }
            }
        }
    }
}
