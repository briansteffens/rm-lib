Shop\RSS File Format
====================

```
Redmoon server file: Shop\Shop00*.rss
From Redmoon version: 3.8
Encoding: ASCII
```

Each of these files sets the inventory for a particular shop. Each line in
the file adds an item to the shop inventory. The line format is as follows
(one tab between values):

```
;kind index count price% unknown
1      10    50    100      3
```

Where:

```
kind - The item kind (1=weapons, 4=food, etc)
index - The item index (1=goldfish, 241=calabolg)
count - The number of items available in the shop to start.
        4000000001 (four billion and one) for no limit.
price% - The percentage of the default price for the item, default 100.
unknown - Presently unknown, this seems to always be 3 except for Silver, Gold,
          Pearl, Diamond, and Mithril which have a value of 2.
```
