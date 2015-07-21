EventInfo.rse
=============

This server file is located at `Data\Event\EventInfo.rse`

Each line has the following minimal format. Each value should be separated 
by one tab character:

```
;event   command 
53         34
```

The first field sets the ID of the event being defined. This is the value 
which will be used in other files (ex: RME files) to reference this event.

The second field sets the command to run. This controls not only what the
event will do but how many other fields are expected on the line.

Partial list of commands (google translated comments from the file, accuracy 
not confirmed):
```
1 - Map move (warp)
2 - Message board
4 - Mail
5 - Safety zone
6 - Bank inventory
7 - PK off
8 - Doctor/nurse heal
9 - Inability to move only the user's character
10 - Duel
11 - Street2 army computer
12 - Free-owned azithromycin group selection window
13 - Street2 army safe
14 - Street2 army board
15 - Street3 army computer
16 - Azithromycin elevator
20 - Exchange
21 - MP charging bay
22 - Siren events
23 - Money box
24 - Poultices box
25 - Weapon box
26 - Money, poultices, weapon boxes
27 - Archaeologist quest
28 - Geographer quest
30 - PK time limit
31 - Stalls shops
32 - Aunt neighborhood shops
33 - Shopkeeper's shops
34 - Runaway girl store
35 - Scientific research store
36 - Night guards wanted stores
37 - Geographer downtown shops
39 - Temple
40 - Addiction pulley tile
42 - Ocean
43 - Warden (prison movement)
44 - Recalls quest
45 - Go to the battle zone
80 - Sig bank
81 - Sig hospital
82 - Sig Mugijeom
83 - Sig armor points
84 - Sig grocery store
85 - Sig pharmacy
86 - Sig used merchant
370 - Management funds at the base of freedom
371 - Goods Administration at the base of freedom
```

In this example, event ID 53 is set to show the "Runaway girl store" (34) with
shop inventory 3 (defined at Data\Shop\Shop00003.rss).
```
53  34  3
```
