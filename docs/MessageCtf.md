CTF File Format
===============

```
Redmoon client file: Message.ctf
File type/version: J.C.TypeConvTool Ver 1.1.2
From Redmoon version: 3.8
Byte order: little-endian
```

This file handles string localization for the Redmoon client.

It is divided into 3 high level sections: header, messages, and categories.


##Header

The header for Message.ctf is a static string giving the file type and 
version info. In this case it is "J.C.TypeConvTool Ver 1.1.2".


##Messages section

The messages section starts with a uint giving the total number of messages
in the file.

Each message then follows sequentially, with the format:

```
    string string_id   - This seems to be an ASCII representation of the 3
                         uints below, separated by ASCII tabs. I don't know
                         what this is for or which representation is meant to
                         be authoritative.
    uint   category    - The category id to which the message belongs.
                         Ex: 2 = skills, 4 = weapons
    uint   subcategory - The subcategory id (within the category specified
                         above) to which the message belongs.
                         Ex: 0 = swords, 4 = wands
    uint   id          - The message ID, unique within the subcategory
    string message     - The actual message data
```


##Categories section

The categories section defines the categories and subcategories available
for organizing messages.

This section begins with a uint giving one less than the total number of 
categories to follow. So a '9' actually means 10 categories are in the list 
(0-9).

Next is the list of categories. Each category begins with a string of the
category id and name separated by an underscore (Ex: "2_Skills").

This is followed by a uint giving one less than the total number of 
subcategories in this category.

The subcategories are then listed. Each subcategory is a single string with
the subcategory id and name separated by an underscore (Ex: "7_Kitara").
