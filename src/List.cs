using System;
using System.Collections.Generic;
using System.IO;

namespace RedMoon.v38
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message)
            : base(message)
        {
        }
    }

    public class ListItem
    {
        // Descriptive name
        public string Name { get; set; }

        // Item ID
        public int ID { get; set; }

        // The RLE file the resource is located in
        public int File { get; set; }

        // The index of the resource within the RLE file
        public int Index { get; set; }
    }

    // Represents a resource list (*.lst) file
    public class List
    {
        public List<ListItem> Items { get; protected set; }

        public List()
        {
            Items = new List<ListItem>();
        }

        public List(string filename)
            : this()
        {
            Load(filename);
        }

        public void Load(string filename)
        {
            var br = new ByteReader(File.ReadAllBytes(filename));

            if (br.STRING() != "RedMoon Lst File" || br.STRING() != "1.0")
            {
                throw new InvalidFileException();
            }

            // TODO: Unknown value
            br.UINT();

            Items.Clear();
            uint count = br.UINT();

            for (int i = 0; i < count; i++)
            {
                Items.Add(new ListItem()
                {
                    Name = br.STRING(),
                    ID = (int)br.UINT(),
                    File = (int)br.UINT(),
                    Index = (int)br.UINT()
                });
            }
        }

        public ListItem Get(int id)
        {
            foreach (var item in Items)
            {
                if (item.ID == id)
                {
                    return item;
                }
            }

            throw new NotFoundException(string.Format(
                    "List item ID {0} not found.", id));
        }
    }
}
