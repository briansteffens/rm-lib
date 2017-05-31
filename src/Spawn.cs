using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;


namespace RedMoon {


// The AI a mob will use, based on a description apparently by Rapap.
public enum AIPattern
{
    Normal = 0,
    RunAway = 1,
    Assassin = 2,
    Frozen = 3,
    YellowName = 4,
    Super = 5,
    RandomWalking = 6,
    FollowsIntoSafeZones = 7
}


// Represents one drop entry in a mop file line, giving the mobs defined by
// that line a 20% chance to drop [Count] number of [Item]s.
public abstract class MopDropBase
{
    public ItemID Item { get; set; }
    public uint Count { get; set; }
}

// Represents one line in a Mop00###.rsm file, which defines a mob, quantity,
// spawn time, region, AI, and drops to appear on whichever file this line
// belongs to.
public abstract class MopLineBase<TDrop> : TextLine
    where TDrop : MopDropBase, new()
{
    [TextField(0)] public string Comment { get; set; }
    [TextField(1)] public uint MonsterIndex { get; set; }
    [TextField(2)] public uint Left { get; set; }
    [TextField(3)] public uint Top { get; set; }
    [TextField(4)] public uint Right { get; set; }
    [TextField(5)] public uint Bottom { get; set; }
    [TextField(6)] public AIPattern Pattern { get; set; }
    [TextField(7)] public uint Quantity { get; set; }
    [TextField(8)] public uint RespawnTime { get; set; }

    public List<TDrop> Drops { get; set; }

    public MopLineBase()
    {
        Drops = new List<TDrop>();
    }

    protected override List<string> LoadValues(List<string> values)
    {
        Drops.Clear();

        int total_drops = int.Parse(values[6]);
        for (int i = 0; i < total_drops; i++)
        {
            int OFFSET = 7 + (i * 3);

            Drops.Add(new TDrop
            {
                Item = new ItemID(uint.Parse(values[OFFSET + 0]),
                                  uint.Parse(values[OFFSET + 1])),
                Count = uint.Parse(values[OFFSET + 2])
            });
        }

        for (int i = 0; i < total_drops * 3 + 1; i++)
            values.RemoveAt(6);

        return base.LoadValues(values);
    }

    protected override List<string> SaveValues(List<string> values)
    {
        var ret = base.SaveValues(values);

        ret.Insert(6, Drops.Count.ToString());

        for (int i = 0; i < Drops.Count; i++)
            ret.InsertRange(7 + (i * 3), new string[] {
                Drops[i].Item.Kind.ToString(),
                Drops[i].Item.Index.ToString(),
                Drops[i].Count.ToString()});

        return ret;
    }
}

// Represents a Mop00###.rsm file, which consists of a number of lines, each
// configuring a mob spawn.
public abstract class MopFileBase<TLine, TDrop> : TextFile<TLine>
    where TLine : MopLineBase<TDrop>, new()
    where TDrop : MopDropBase, new()
{ }


}



namespace RedMoon.v38 {


public class MOP : MopFileBase<MOP.Line, MOP.Drop>
{
    public class Drop : MopDropBase { }
    public class Line : MopLineBase<Drop> { }
}


}
