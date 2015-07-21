using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using NUnit.Framework;

namespace RM.Tests {



static class COMMON
{
    // Does a hash check and fails if the two byte buffers are not identical.
    public static void CompareFiles(byte[] file, ByteWriter bw)
    {
        Assert.AreEqual(HASH.SHA256(new MemoryStream(file)),
                        HASH.SHA256(new MemoryStream(bw.ToByteArray())));
    }

    public static void IsGoldfish(ItemBase item)
    {
        Assert.AreEqual(4, item.ID.Kind);
        Assert.AreEqual(1, item.ID.Index);
        Assert.AreEqual(75, item.HP);
        Assert.AreEqual(0, item.MP);
        Assert.AreEqual(0, item.Attack);
        Assert.AreEqual(0, item.Defense); 
        Assert.AreEqual(0, item.Strength);
        Assert.AreEqual(0, item.Spirit);
        Assert.AreEqual(0, item.Dexterity);
        Assert.AreEqual(0, item.Power);
        Assert.AreEqual(0, item.RequiredLevel);
        Assert.AreEqual(0, item.RequiredStrength);
        Assert.AreEqual(0, item.RequiredSpirit);
        Assert.AreEqual(0, item.RequiredDexterity);
        Assert.AreEqual(0, item.RequiredPower);
        Assert.AreEqual(0, item.Unused0);
        Assert.AreEqual(0, item.Characters);
        Assert.AreEqual(1, item.Weight);
        Assert.AreEqual(0, item.Unused1);
        Assert.AreEqual(14, item.Slot);
        Assert.AreEqual(0, item.Unused2);
        Assert.AreEqual(0, item.Formula);
        Assert.AreEqual(0, item.Range);
        Assert.AreEqual(0, item.ScatterRange);
        Assert.AreEqual(0, item.Animation);
        Assert.AreEqual(50, item.Price);
    }

    public static void IsBeer(ItemBase item)
    {
        Assert.AreEqual(0, item.HP);
        Assert.AreEqual(400, item.MP);
        Assert.AreEqual(14, item.Slot);
        Assert.AreEqual(200, item.Price);
    }

    public static void IsNovaBlade(ItemBase item)
    {
        Assert.AreEqual(108000, item.Attack);
        Assert.AreEqual(5000, item.Defense);
        Assert.AreEqual(2000, item.Dexterity);
        Assert.AreEqual(400, item.Power);
        Assert.AreEqual(650, item.RequiredLevel);
        Assert.AreEqual(40, item.Weight);
        Assert.AreEqual(204, item.Characters);
        Assert.AreEqual(7, item.Slot);
        Assert.AreEqual(1, item.Range);
        Assert.AreEqual(1000000000, item.Price);
    }

    public static void IsNovaWand(ItemBase item)
    {
        Assert.AreEqual(500, item.Attack);
        Assert.AreEqual(3000, item.Defense);
        Assert.AreEqual(500, item.Strength);
        Assert.AreEqual(3500, item.Spirit);
        Assert.AreEqual(10, item.Weight);
        Assert.AreEqual(320, item.Characters);
        Assert.AreEqual(7, item.Slot);
        Assert.AreEqual(1, item.Range);
        Assert.AreEqual(500000000, item.Price);
    }

    public static void IsM9(ItemBase item)
    {
        Assert.AreEqual(8, item.Range);
        Assert.AreEqual(2, item.ScatterRange);
    }
}



[TestFixture]
public class ItemsTests
{
    [Test]
    public void Test_38_RMI_Kind4()
    {
        byte[] source_bytes = File.ReadAllBytes("testdata/38/Item04.rmi");
        var rmi = new RM.v38.RMI(new ByteReader(source_bytes));

        Assert.AreEqual(4, rmi.ItemKind);
        Assert.AreEqual(46, rmi.Items.Count);

        COMMON.IsGoldfish(rmi.Items[1]);
        COMMON.IsBeer(rmi.Items[22]);
        
        COMMON.CompareFiles(source_bytes, rmi.Save(new ByteWriter()));
    }

    [Test]
    public void Test_38_RMI_Kind1()
    {
        byte[] source_bytes = File.ReadAllBytes("testdata/38/Item01.rmi");
        var rmi = new RM.v38.RMI(new ByteReader(source_bytes));

        Assert.AreEqual(1, rmi.ItemKind);
        Assert.AreEqual(242, rmi.Items.Count);

        COMMON.IsNovaBlade(rmi.Items[154]);
        Assert.AreEqual(new CTF.Ref(4,0,42), rmi.Items[154].Message);
               
        COMMON.IsNovaWand(rmi.Items[164]);
        Assert.AreEqual(new CTF.Ref(4,4,26), rmi.Items[164].Message);
        
        COMMON.IsM9(rmi.Items[146]);
        Assert.AreEqual(new CTF.Ref(4,5,7), rmi.Items[146].Message);
        
        COMMON.CompareFiles(source_bytes, rmi.Save(new ByteWriter())); 
    }

    [Test]
    public void Test_38_RSI_Kind4()
    {
        byte[] source_bytes = File.ReadAllBytes("testdata/38/ItemInfo04.rsi");
        var rsi = new RM.v38.RSI(new ByteReader(source_bytes, ENC.KR));

        Assert.AreEqual(4, rsi.ItemKind);
        Assert.AreEqual(46, rsi.Items.Count);

        COMMON.IsGoldfish(rsi.Items[1]);
        COMMON.IsBeer(rsi.Items[22]);

        COMMON.CompareFiles(source_bytes, rsi.Save(new ByteWriter(ENC.KR)));
    }

    [Test]
    public void Test_38_RSI_Kind1()
    {
        byte[] source_bytes = File.ReadAllBytes("testdata/38/ItemInfo01.rsi");
        var rsi = new RM.v38.RSI(new ByteReader(source_bytes, ENC.KR));

        Assert.AreEqual(1, rsi.ItemKind);
        Assert.AreEqual(242, rsi.Items.Count);

        var item = rsi.Items[154];
        COMMON.IsNovaBlade(item);
        Assert.AreEqual("선 블레이드+2", item.ShopDescription);
        Assert.AreEqual("선 블레이드+1의 강화판", item.InventoryDescription);
        
        item = rsi.Items[164];
        COMMON.IsNovaWand(item);
        Assert.AreEqual("선 완드+3", item.ShopDescription);
        Assert.AreEqual("선 완드+2의 강화판", item.InventoryDescription);

        item = rsi.Items[146];
        COMMON.IsM9(item);
        Assert.AreEqual("바주카포+1", item.ShopDescription);
        Assert.AreEqual("바주카포의 강화판", item.InventoryDescription);

        COMMON.CompareFiles(source_bytes, rsi.Save(new ByteWriter(ENC.KR)));
    }

}


}
