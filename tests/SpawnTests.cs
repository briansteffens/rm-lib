using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using NUnit.Framework;

using RM;

namespace RM.Tests {


[TestFixture]
public class SpawnTests
{ 
    [Test]
    public void Test_v38_MOP()
    {
        var file = new RM.v38.MOP();
        
        string source = File.ReadAllText("testdata/38/Mop00137.rsm");
        file.Load(source);

        Assert.AreEqual(20, file.AllItems.Count);
        Assert.AreEqual(12, new List<object>(file.Items).Count);

        // A standard line with a single drop
        var it = file.Items[3];
        Assert.AreEqual("SkyCop", it.Comment);
        Assert.AreEqual(65, it.MonsterIndex);
        Assert.AreEqual(15, it.Left);
        Assert.AreEqual(20, it.Top);
        Assert.AreEqual(220, it.Right);
        Assert.AreEqual(355, it.Bottom);
        Assert.AreEqual(1, it.Drops.Count);
        Assert.AreEqual(4, it.Drops[0].Item.Kind);
        Assert.AreEqual(13, it.Drops[0].Item.Index);
        Assert.AreEqual(2, it.Drops[0].Count);
        Assert.AreEqual(AIPattern.Normal, it.Pattern);
        Assert.AreEqual(100, it.Quantity);
        Assert.AreEqual(120, it.RespawnTime);

        // No drops
        it = file.Items[4];
        Assert.AreEqual(0, it.Drops.Count);
        Assert.AreEqual("Annihilator", it.Comment);
        Assert.AreEqual(70, it.Quantity);

        // Multiple drops
        it = file.Items[6];
        Assert.AreEqual(2, it.Drops.Count);
        Assert.AreEqual("Psyanide", it.Comment);
        Assert.AreEqual(45, it.Quantity);

        // Comment
        Assert.AreEqual(" Main spawn", ((CommentLine)file.AllItems[0]).Comment);

        Assert.AreEqual(source, file.Save());
    }
}


}
