using System;
using System.IO;
using System.Text;

using NUnit.Framework;

using RM;

namespace RM.Tests {


[TestFixture]
public class MessageCtfTests
{
    [SetUp]
    public void Init()
    {
    }

    [Test]
    public void Test_CTF()
    {
        byte[] source_buf = File.ReadAllBytes("testdata/Message.ctf");
        var target = new CTF.File(new ByteReader(source_buf));

        Assert.AreEqual(3204, target.Messages.Count);

        Assert.AreEqual("UniCastle", target.GetMessage(0, 6, 312).Text);
        Assert.AreEqual("Frost Staff@", target.GetMessage(4, 4, 22).Text);

        Assert.AreEqual(10, target.Categories.Count);

        Assert.AreEqual("Skills", target.Categories[2].Name);
        Assert.AreEqual("Items1:Weapons", target.Categories[4].Name);

        Assert.AreEqual("Kitara", target.Categories[2][7].Name);
        Assert.AreEqual("Wand", target.Categories[4][4].Name);

        Assert.AreEqual(
            HASH.SHA256(new MemoryStream(source_buf)),
            HASH.SHA256(new MemoryStream(target.Save(
                        new ByteWriter()).ToByteArray())));
        
    }
}


}
