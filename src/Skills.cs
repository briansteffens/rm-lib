using System;
using System.IO;
using System.Collections.Generic;

namespace RM
{
    public enum ManaUsageStyle
    {
        Fixed,
        Percentage
    }

    public enum SkillType
    {
        Normal = 0,
        Special = 1,
        Monster = 2
    }

    public class Skill
    {
        public SkillType Type { get; set; }
        public CTF.Message Message { get; set; }
        public uint IconIndex { get; set; }
        public ushort ProjectileAnimation { get; set; }
        public uint CharacterAnimation { get; set; }
        public int Honor { get; set; }
        public uint Level { get; set; }
        public byte CharacterType { get; set; }
        public ManaUsageStyle ManaUsageStyle { get; set; }
        public uint ManaUsage { get; set; }
        public bool IsAOE { get; set; }
        public short Range { get; set; }
        public byte NeedsTarget { get; set; }
        public byte Unused1 { get; set; }
        public byte Unused2 { get; set; }
        public byte Unused3 { get; set; }
        public byte EffectType { get; set; }
        public uint Multiplier { get; set; }
        public uint Unknown5 { get; set; }

        public string Name
        {
            get
            {
                return Message.Text.Split('@')[0];
            }
        }

        public string Description
        {
            get
            {
                return Message.Text.Split('@')[1];
            }
        }

        public byte[] Raw { get; set; }
        public Skill()
        {
            Raw = new byte[64];
        }
    }

    public class SkillFile
    {
        public static string FileID = "RedMoon Skill Info File V 1.0";

        public string FileName { get; private set; }
        public List<Skill> Skills { get; private set; }

        public SkillFile(CTF.File ctf, string filename)
        {
            FileName = filename;
            var br = new ByteReader(File.ReadAllBytes(FileName));

            if (br.STRING() != FileID)
            {
                throw new Exception("Invalid file format.");
            }

            var messages = new List<CTF.Message>();

            foreach (var msg in ctf.Messages)
            {
                if (msg.SubCategory.Category.Name != "Skills")
                {
                    continue;
                }

                messages.Add(msg);
            }

            var skill_count = br.UINT();

            Skills = new List<Skill>();

            for (int i = 0; i < skill_count; i++)
            {
                var s = new Skill();

                s.Type = (SkillType)br.BYTE();

                var message_id = br.STRING();

                if (message_id == "")
                {
                    Skills.Add(null);
                    br.Skip(39);
                    continue;
                }

                var class_id = br.STRING();

                foreach (var message in messages)
                {
                    if (message.SubCategory.ID == uint.Parse(class_id) &&
                        message.ID == uint.Parse(message_id))
                    {
                        s.Message = message;
                        break;
                    }
                }

                if (s.Message == null)
                {
                    throw new Exception("Failed to find message.");
                }

                s.IconIndex = br.UINT();
                s.ProjectileAnimation = br.USHORT();
                s.CharacterAnimation = br.UINT();
                s.Honor = br.INT();
                s.Level = br.UINT();
                s.CharacterType = br.BYTE();

                var mana = br.UINT();
                if (mana > 1000000000)
                {
                    s.ManaUsageStyle = ManaUsageStyle.Percentage;
                    s.ManaUsage = mana - 1000000000;
                }
                else
                {
                    s.ManaUsageStyle = ManaUsageStyle.Fixed;
                    s.ManaUsage = mana;
                }

                var range = br.SHORT();
                if (range > 10000)
                {
                    s.IsAOE = true;
                    s.Range = (short)(range - 10000);
                }
                else
                {
                    s.IsAOE = false;
                    s.Range = range;
                }

                s.NeedsTarget = br.BYTE();

                s.Unused1 = br.BYTE();
                s.Unused2 = br.BYTE();
                s.Unused3 = br.BYTE();

                s.EffectType = br.BYTE();
                s.Multiplier = br.UINT();

                s.Unknown5 = br.UINT();

                Skills.Add(s);
            }
        }

        public void Save(string filename=null)
        {
            var bw = new ByteWriter();

            bw.STRING(FileID);

            bw.UINT((uint)Skills.Count);

            foreach (var s in Skills)
            {
                // Handle empty entries
                if (s == null)
                {
                    for (int i = 0; i < 41; i++)
                    {
                        bw.BYTE(0);
                    }

                    continue;
                }

                bw.BYTE((byte)s.Type);
                bw.STRING(s.Message.ID.ToString());
                bw.STRING(s.Message.SubCategory.ID.ToString());
                bw.UINT(s.IconIndex);
                bw.USHORT(s.ProjectileAnimation);
                bw.UINT(s.CharacterAnimation);
                bw.INT(s.Honor);
                bw.UINT(s.Level);
                bw.BYTE(s.CharacterType);

                uint mana = s.ManaUsage;
                if (s.ManaUsageStyle == ManaUsageStyle.Percentage)
                {
                    mana += 1000000000;
                }
                bw.UINT(mana);

                short range = s.Range;
                if (s.IsAOE)
                {
                    range += 10000;
                }
                bw.SHORT(range);

                bw.BYTE(s.NeedsTarget);

                bw.BYTE(s.Unused1);
                bw.BYTE(s.Unused2);
                bw.BYTE(s.Unused3);

                bw.BYTE(s.EffectType);
                bw.UINT(s.Multiplier);

                bw.UINT(s.Unknown5);
            }

            File.WriteAllBytes(filename ?? FileName, bw.ToByteArray());
        }
    }
}
