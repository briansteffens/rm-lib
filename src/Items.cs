using System;
using System.Collections.Generic;


namespace RM {


public struct ItemID
{
    public uint Kind { get; set; }
    public uint Index { get; set; }

    public ItemID(uint kind, uint index) : this()
    {
        this.Kind = kind;
        this.Index = index;
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public override bool Equals(object other)
    {
        return other != null && other is ItemID && 
               ToString() == other.ToString();
    }

    public override string ToString()
    {
        return string.Format("{0}.{1}", Kind, Index);
    }
}


public abstract class ItemBase
{
    public ItemID ID { get; set; }    
    public uint ImageIndex { get; set; }
    
    public uint HP { get; set; }
    public uint MP { get; set; }
    public uint Attack { get; set; }
    public uint Defense { get; set; }
    public uint Strength { get; set; }
    public uint Spirit { get; set; }
    public uint Dexterity { get; set; }
    public uint Power { get; set; }
    public uint RequiredLevel { get; set; }
    public uint RequiredStrength { get; set; }
    public uint RequiredSpirit { get; set; }
    public uint RequiredDexterity { get; set; }
    public uint RequiredPower { get; set; }
    public uint Unused0 { get; set; }
    public ushort Characters { get; set; }
    public ushort Weight { get; set; }
    public ushort Unused1 { get; set; }
    public ushort Slot { get; set; }
    public ushort Unused2 { get; set; }
    public byte Formula { get; set; }
    public byte Range { get; set; }
    public byte ScatterRange { get; set; }
    public byte Animation { get; set; }
    public uint Price { get; set; }
}

public abstract class ItemFileBase<T> : RedmoonBinaryFile where T : ItemBase
{
    public uint ItemKind { get; set; }
    public List<T> Items { get; protected set; }

    public override string VERSION_STRING 
        { get { return "RedMoon ItemInfo File 1.0"; } }

    public ItemFileBase(ByteReader br) : base(br) { }

    protected override void Load(ByteReader br)
    {
        base.Load(br);

        ItemKind = br.UINT();

        Items = new List<T>();

        uint total_items = br.UINT() + 1;
        for (uint i = 0; i < total_items; i++)
            Items.Add(LoadItem(i, br));
    }

    protected abstract T CreateItem();

    protected virtual T LoadItem(uint index, ByteReader br)
    {
        var item = CreateItem();

        item.ID = new ItemID(br.UINT(), index);
        if (item.ID.Kind == 0)
        {
            br.Offset += 80;
            return null;
        }

        item.ImageIndex = br.UINT();
        
        ReadVariableFields(br, item);
        
        item.HP = br.UINT();
        item.MP = br.UINT();
        item.Attack = br.UINT();
        item.Defense = br.UINT();
        item.Strength = br.UINT();
        item.Spirit = br.UINT();
        item.Dexterity = br.UINT();
        item.Power = br.UINT();
        item.RequiredLevel = br.UINT();
        item.RequiredStrength = br.UINT();
        item.RequiredSpirit = br.UINT();
        item.RequiredDexterity = br.UINT();
        item.RequiredPower = br.UINT();
        item.Unused0 = br.UINT();
        item.Characters = br.USHORT();
        item.Weight = br.USHORT();
        item.Unused1 = br.USHORT();
        item.Slot = br.USHORT();
        item.Unused2 = br.USHORT();
        item.Formula = br.BYTE();
        item.Range = br.BYTE();
        item.ScatterRange = br.BYTE();
        item.Animation = br.BYTE();
        item.Price = br.UINT();
        
        return item;
    }

    public override ByteWriter Save(ByteWriter bw)
    {
        base.Save(bw);

        bw.UINT(ItemKind);
        bw.UINT((uint)Items.Count - 1);
        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];

            if (item == null)
            {
                for (int j = 0; j < 84; j++)
                    bw.BYTE(0);

                continue;
            }

            bw.UINT(item.ID.Kind);
            bw.UINT(item.ImageIndex);

            SaveVariableFields(bw, item);

            bw.UINT(item.HP);
            bw.UINT(item.MP);
            bw.UINT(item.Attack);
            bw.UINT(item.Defense);
            bw.UINT(item.Strength);
            bw.UINT(item.Spirit);
            bw.UINT(item.Dexterity);
            bw.UINT(item.Power);
            bw.UINT(item.RequiredLevel);
            bw.UINT(item.RequiredStrength);
            bw.UINT(item.RequiredSpirit);
            bw.UINT(item.RequiredDexterity);
            bw.UINT(item.RequiredPower);
            bw.UINT(item.Unused0);
            bw.USHORT(item.Characters);
            bw.USHORT(item.Weight);
            bw.USHORT(item.Unused1);
            bw.USHORT(item.Slot);
            bw.USHORT(item.Unused2);
            bw.BYTE(item.Formula);
            bw.BYTE(item.Range);
            bw.BYTE(item.ScatterRange);
            bw.BYTE(item.Animation);
            bw.UINT(item.Price);
        }

        return bw;
    }

    // This is called by LoadItem() after the first two item fields have been
    // loaded from the ByteReader. This is because the client and server
    // versions of the item file format differ here.
    protected abstract void ReadVariableFields(ByteReader br, T item);

    // This is called when saving, after an item's first two fields have been
    // written to the ByteWriter.
    protected abstract void SaveVariableFields(ByteWriter bw, T item);
}

}


namespace RM.v38 {


public class RMI : ItemFileBase<RMI.Item>
{
    public class Item : ItemBase
    {
        public CTF.Ref Message { get; set; }
    }

    public RMI(ByteReader br) : base(br) {}

    protected override Item CreateItem() { return new Item(); }

    protected override void ReadVariableFields(ByteReader br, Item item)
    {
        // Read the Message.ctf reference #
        item.Message = new CTF.Ref(
            item.ID.Kind + 3, // offset to convert from kind to CTF category
            uint.Parse(br.STRING()),
            uint.Parse(br.STRING()));
    }

    protected override void SaveVariableFields(ByteWriter bw, Item item)
    {
        // Write the Message.ctf reference #
        bw.STRING(item.Message.SubCategoryID.ToString());
        bw.STRING(item.Message.ID.ToString());
    }
}


public class RSI : ItemFileBase<RSI.Item>
{
    public class Item : ItemBase
    {
        public string ShopDescription { get; set; }
        public string InventoryDescription { get; set; }
    }

    public RSI(ByteReader br) : base(br) {}

    protected override Item CreateItem() { return new Item(); }

    protected override void ReadVariableFields(ByteReader br, Item item)
    {
        item.ShopDescription = br.STRING();
        item.InventoryDescription = br.STRING();
    }

    protected override void SaveVariableFields(ByteWriter bw, Item item)
    {
        bw.STRING(item.ShopDescription);
        bw.STRING(item.InventoryDescription);
    } 
}


}

