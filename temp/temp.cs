using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;

using RM;
using RM.v38;

struct MapInfo
{
    public uint Map;
    public uint Width;
    public uint Height;
    public bool Fat;
    public uint[] Mobs;

    public MapInfo(uint map, uint width, uint height, bool fat, uint[] mobs)
    {
        Map = map;
        Width = width;
        Height = height;
        Fat = fat;
        Mobs = mobs;
    }

    public static MapInfo[] All()
    {
        return new MapInfo[] {
            new MapInfo(0,149,199,false,new uint[]{4,5,6,9,39}),
            new MapInfo(1,149,199,false,new uint[]{2,3,4,8}),
            new MapInfo(2,99,149,false,new uint[]{4,5,6,9,14}),
            new MapInfo(3,249,249,false,new uint[]{6,39,10}),
            new MapInfo(71,199,199,false,new uint[]{11,12,29,13}),
            new MapInfo(72,119,159,false,new uint[]{28,30,33,34}),
            
            
            new MapInfo(21,249,249,false,new uint[]{1,2,7}),
            new MapInfo(29,199,199,false,new uint[]{3,8,14}),
            new MapInfo(36,249,249,false,new uint[]{11,12,29,30}),
            new MapInfo(37,249,249,false,new uint[]{6,39,10,11}),
            new MapInfo(38,249,249,false,new uint[]{39,10,11}),
            new MapInfo(39,249,249,false,new uint[]{39,10,11,12}),
            


            new MapInfo(76,119,159,true,new uint[]{34,35,36,37}),
            new MapInfo(77,159,209,true,new uint[]{33,34,35,36,37}),
            new MapInfo(79,199,199,true,new uint[]{44,42,40}),
            new MapInfo(83,119,159,true,new uint[]{40,41}),
            new MapInfo(84,119,159,true,new uint[]{40,41}),
            new MapInfo(85,119,159,true,new uint[]{41,43}),
            new MapInfo(86,119,159,true,new uint[]{41,43}),
             
            new MapInfo(93,119,159,true,new uint[]{53,54,57}),
            new MapInfo(94,119,159,true,new uint[]{53,54,57}),
            new MapInfo(95,119,159,true,new uint[]{53,54,57}),
            new MapInfo(96,119,159,true,new uint[]{53,54,57}),



            new MapInfo(137,229,359,true,new uint[]{64,65,66,67}),
            new MapInfo(138,229,359,true,new uint[]{64,65,66,67}),
            new MapInfo(139,229,359,true,new uint[]{64,65,66,67}),
            new MapInfo(140,229,359,true,new uint[]{64,65,66,67}),
            new MapInfo(141,229,359,true,new uint[]{64,65,66,67}),

            new MapInfo(177,159,209,true,new uint[]{28,29,30,33,68,69}),
            new MapInfo(178,159,209,true,new uint[]{28,33,70,71,72,73}),
            new MapInfo(179,159,209,true,new uint[]{73,76}),

        };
    }
}

class CONFIG
{
    public const string SERVER_PATH = "/mnt/rmserver/current/";
    public const string CLIENT_PATH = "/mnt/rmclient/";
}

abstract class InstanceContext
{
    public string Path { get; set; }

    public InstanceContext(string path)
    {
        Path = path;
    }
}

class ClientContext : InstanceContext
{
    public ClientContext(string path) : base(path) {}


    public string CTFPath { get { return Path + "DATAs/Info/Message.ctf"; } }

    CTF.File ctf;

    public CTF.File CTF { get {
        if (ctf == null)
            ctf = new CTF.File(new ByteReader(File.ReadAllBytes(CTFPath)));

        return ctf;
    } }


    public string RMIPath(int item_kind)
    {
        return string.Format("{0}DATAs/Info/Item{1}.rmi", 
                             Path, item_kind.ToString().PadLeft(2, '0'));
    }

    Dictionary<int, RMI> rmis = new Dictionary<int, RMI>();

    public RMI RMI(int item_kind)
    {
        if (!rmis.ContainsKey(item_kind))
            rmis.Add(item_kind, new RMI(new ByteReader(File.ReadAllBytes(
                                        RMIPath(item_kind)))));

        return rmis[item_kind];
    }
}

class ServerContext : InstanceContext
{
    public ServerContext(string path) : base(path) {}


    public string RSIPath(int item_kind)
    {
        return string.Format("{0}Data/Item/Info/ItemInfo{1}.rsi",
                             Path, item_kind.ToString().PadLeft(2, '0'));
    }

    Dictionary<int, RSI> rsis = new Dictionary<int, RSI>();

    public RSI RSI(int item_kind)
    {
        if (!rsis.ContainsKey(item_kind))
            rsis.Add(item_kind, new RSI(new ByteReader(File.ReadAllBytes(
                                        RSIPath(item_kind)), ENC.KR)));

        return rsis[item_kind];
    }
}

class SpawnInfo
{
    public MapInfo MapInfo { get; set; }
    public List<ItemID> Items { get; set; }
    public float QuantityDivisor { get; set; }
    public string Comment { get; set; }

    public SpawnInfo()
    {
        Items = new List<ItemID>();
    }

    public void Spawn()
    {
        var rand = new Random();

        uint area = MapInfo.Width * MapInfo.Height;

        var mop = new RM.v38.MOP();

        string fn = CONFIG.SERVER_PATH + "Data/Mop/Mop" + 
                    MapInfo.Map.ToString().PadLeft(5, '0') + ".rsm";
        Console.WriteLine(fn);
        mop.Load(File.ReadAllText(fn));

        while (true)
        {
            if (mop.AllItems.Count > 1 &&
                mop.AllItems[mop.AllItems.Count - 1] is BlankLine &&
                mop.AllItems[mop.AllItems.Count - 2] is BlankLine)
            break;

            mop.AllItems.Add(new BlankLine());
        }

        mop.AllItems.Add(new CommentLine(" BEGIN [" + Comment + "]"));

        uint quantity = (uint)Math.Ceiling((float)area / QuantityDivisor);
        if (quantity < 1) quantity = 1;

        foreach (ItemID item in Items)
        {
            var l = new MOP.Line() {
                Comment = "?",
                MonsterIndex = MapInfo.Mobs[rand.Next(MapInfo.Mobs.Length)],
                Left = 5,
                Top = 5,
                Right = MapInfo.Width - 5,
                Bottom = MapInfo.Height - 5,
                Pattern = AIPattern.Normal,
                Quantity = quantity,
                RespawnTime = (uint)rand.Next(180, 360),
            };

            int chance = rand.Next(5);
            for (int i = 0; i < chance; i++)
                l.Drops.Add(new MOP.Drop() {
                    Item = item,
                    Count = 1,
                });

            mop.AllItems.Add(l);
        }

        mop.AllItems.Add(new CommentLine(" END [" + Comment + "]"));
        mop.AllItems.Add(new BlankLine());

        File.WriteAllText(fn, mop.Save());
    }
}

class ItemEditing
{
    public static void CTF()
    {
        string fn = CONFIG.CLIENT_PATH + "DATAs/Info/Message.ctf";
        var ctf = new CTF.File(new ByteReader(File.ReadAllBytes(fn)));

        ctf.Messages.Add(new CTF.Message() {
            Ref = new CTF.Ref(6,5,700),
            Text = "Englor@"
        });
        
        File.WriteAllBytes(fn, ctf.Save(new ByteWriter()).ToByteArray());
    }

    public static void RSI()
    {
        string fn = CONFIG.SERVER_PATH + "Data/Item/Info/ItemInfo03.rsi";
        var rsi = new RSI(new ByteReader(File.ReadAllBytes(fn), ENC.KR));

        var item = new RSI.Item()
        {
            ShopDescription = "",
            InventoryDescription = ""
        };

        SetCommonItemFields(item);

        rsi.Items.Add(item);

        File.WriteAllBytes(fn, rsi.Save(new ByteWriter(ENC.KR)).ToByteArray());
    }

    public static void RMI()
    {
        string fn = CONFIG.CLIENT_PATH + "DATAs/Info/Item03.rmi";
        var rmi = new RMI(new ByteReader(File.ReadAllBytes(fn)));

        var item = new RMI.Item();
        item.Message = new CTF.Ref(6,5,700);

        SetCommonItemFields(item);

        rmi.Items.Add(item);

        File.WriteAllBytes(fn, rmi.Save(new ByteWriter()).ToByteArray());
    }

    static void SetCommonItemFields(ItemBase item)
    {
        item.ID = new ItemID(3, 305);
        item.ImageIndex = 0;

        item.Strength = 30;
        item.Dexterity = 170;
        item.RequiredLevel = 15;
        item.Weight = 1;
        item.Slot = 12;
    }
}

public class Program
{
    static void SpawnBoosters()
    {
        foreach (var mi in MapInfo.All())
        {
            var boosts = new List<uint>(new uint [] { 
                111,112,113,114,115,116,117,118,119,120,148,149 });

            if (mi.Fat)
                boosts.AddRange(new uint[] { 121,122,123,124,125,126,127,128,
                                             129,130,150,151 });

            var items = new List<ItemID>();
            foreach (var boost in boosts)
                items.Add(new ItemID(5, boost));

            new SpawnInfo() {
                MapInfo = mi,
                QuantityDivisor = 30000f,
                Comment = "boosters",
                Items = items
            }.Spawn();
        }
    }

    static void SpawnUniques()
    {
        foreach (var mi in MapInfo.All())
        {
            var unis = new List<uint>(new uint [] { 
                1,2,8,10,11,12,14,15,17,18,23,24,26,28,35,36,37,41,45,67,68,
                80,81,82,83,84,90,91,92,93,94,95,96,97,98,200
            });

            var items = new List<ItemID>();
            foreach (var uni in unis)
                items.Add(new ItemID(6, uni));

            new SpawnInfo() {
                MapInfo = mi,
                QuantityDivisor = 30000f,
                Comment = "uniques",
                Items = items
            }.Spawn();
        }
    }

/*
    static IEnumerable<string> AllFiles(string path, 
                                        string pattern=null, 
                                        bool recursive=false)
    {
        foreach (string file in Directory.GetFiles(

        foreach (string subpath in Directory.GetDirectories(path))
            foreach (string subret in AllFiles(subpath,pattern,recursive))
                yield return subret;
    }
*/

    static void RLE_stuff()
    {
        if (File.Exists("ex.txt"))
            File.Delete("ex.txt");

        if (Directory.Exists("./img"))
            Directory.Delete("./img", true);
        Directory.CreateDirectory("./img");

        foreach (string fn in Directory.GetFiles("/mnt/rmclient/RLEs/Chr", "*.rle", 
                                                 SearchOption.AllDirectories))
        {
            Console.WriteLine(fn);
            //Console.ReadKey();

            var rle = new RM.v38.RLE();
            try
            {
                rle.Load(new ByteReader(File.ReadAllBytes(fn)), fn);
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX IN " + fn);
                throw ex;
            }

            var f = fn.Replace("/mnt/rmclient/RLEs/", "")
                      .Replace("/", "_")
                      .Replace(".rle", "");

            for (int i = 0; i < rle.Resources.Count; i++)
                if (rle.Resources[i] != null && rle.Resources[i].Image != null)
                    rle.Resources[i].Image.Save("./img/" + f + "_" + i + ".bmp");

            Console.WriteLine(f);
        }
    }


    public static void Main(string[] args)
    {
        RLE_stuff();

        //ItemEditing.CTF();
        //ItemEditing.RMI();
                
        //ItemEditing.RSI();
        
        //SpawnUniques();
        //SpawnBoosters();
        
/*

        var client = new ClientContext("/mnt/rmclient/");

        foreach (var item in client.RMI(3).Items)
            if (item != null)
                Console.WriteLine(
                    "{0} {1}, {2}: {3}",
                    item.ID.Index,
                    item.Message,
                    item.ImageIndex,
                    client.CTF.GetMessage(item.Message).Text);
*/


/*
        string fn = CONFIG.CLIENT_PATH + "DATAs/Info/Message.ctf";
        var ctf = new CTF.File(new ByteReader(File.ReadAllBytes(fn)));

        for (uint i = 0; i < 55; i++)
        {
            bool found = false;

            foreach (var m in ctf.Lines)
                if (m.CategoryID == 6 &&
                    m.SubCategoryID == 5 &&
                    m.ID == i)
                {
                    Console.WriteLine("{0}:{1}", m.ID, m.Message);
                    found = true;
                    break;
                }

            if (found) continue;
            Console.WriteLine(i);
        }

        Console.WriteLine(ctf.Lines.Count);
*/
    }
}
