RMI file format
===============

```
Redmoon client file: ./DATAs/Info/Item0*.rmi
File type/version: RedMoon ItemInfo File 1.0
From Redmoon version: 3.8
Encoding: binary
```

These files define the items that are available in-game to the Redmoon client.
A single RMI file can only include items of one category. For example,
`Item01.rmi` for weapons or `Item04.rmi` for food/drink.

Items in this file need matching counterparts in the server-side
[RSI files](/docs/RSI.md) for everything to work.

Files of this type have two major parts: the header and the item data.

# Header

The header starts with a kind of file type + version string. Strings in this
file start with an unsigned 8-bit integer giving the length of the string data
to follow (or, if the string length is greater than 254, this first byte will
be '255' and the actual string length will be a ushort stored in the next 2
bytes of the file). In this case, the first byte in the file, as a ushort,
should be 25. Using that, the ASCII string "RedMoon ItemInfo File 1.0"
(in one example) can be extracted from bytes 1-25.

The next 4 bytes are a uint giving the item kind/category for the file
(1 = weapons, 2 = armor, 6 = special, etc). This probably needs to match the
item kind in the filename (Item0#.rmi).

Then another 4 byte uint giving 1 less than the total number of game items
listed in the file. So in the default Item04.rmi, which has 46 total items, it
lists '45' for this value.

# Item data

Next comes the actual item data. Items are sequential, with their position in
the file controlling which item index they are (1 = goldfish, 3 = hamburger,
etc). Item data length is variable because of 2 string fields in each item.

Some items are 'empty'. An empty item is just like a normal item in the file
but all its values are zeroes and empty strings. These should apparently be
ignored except as placeholders to produce gaps in the item index listings.
For example in a default 3.8 client there is a gap between 41 (Coffee) and
45 (Mind's Eye). In the file, items 42, 43, and 44 are still there but they're
empty. Empty items can be detected easily, just read the first 4 bytes of an
item as a uint and if it's 0, the item is empty and can be ignored.

An item is just a list of values one after another, and they are:

```
uint    item_kind - like in the header and filename (2=armor,4=food)
uint    image - controls which images are used (inventory/ground)
string  message_ctf_subcat - the subcategory id for the CTF ref
string  message_ctf_id - the id for the CTF ref (the name of the item)
uint    hp - the hp added by the item when consumed (food)
uint    mp - the mp added by the item when consumed (drinks)
uint    atk - the attack added by the item when equipped
uint    def - the defense added by the item when equipped
uint    str - the strength added by the item when equipped
uint    spr - the spirit added by the item when equipped
uint    dex - the dexterity added by the item when equipped
uint    pow - the power added by the item when equipped
uint    reqlvl - the level required to equip the item
uint    reqstr - the strength required to equip the item
uint    reqspr - the spirit required to equip the item
uint    reqdex - the dexterity required to equip the item
uint    reqpow - the power required to equip the item
uint    unused0 - apparently unused, always seems to be 0
ushort  characters - some kind of bitmask that controls the characters that can
                     wear the item. 204 = sadad+luna+dest+kit
ushort  weight - the item's weight
ushort  unused1 - apparently unused, always seems to be 0
ushort  slot - the slot used when the item is equipped
ushort  unused2 - apparently unused, always seems to be 0
ubyte   formula - seems to be the item type or attack formula.
                  0 = normal, 1 = sigrare, 3 = sunset (and some unis?)
ubyte   range - the distance in tiles the weapon can attack over
                ex: 1 = swords, 2 = spears, 8 = bows
ubyte   scatter_range - this is used for things like tow launchers, setting a
                        distance from the attacked target within which other
                        enemies will also be hit
ubyte   animation - the skill effect used. by default this is for the unique
                    weapons. 90 = Wand of Gaia effect, 92 = Nemesis Bow, etc
uint    price - the base price for the item in shops
```

Lots of these values only apply to certain items. Generally when a field
doesn't apply to the item in question, its value will be "0". For example only
TOW/M9 have a non-zero value for `scatter_range` by default.
