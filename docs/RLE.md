RLE File Format
===============

```
Redmoon client file: RLEs/Bul/bul*.rle
                     RLEs/Chr/C05/c05*.rle
From Redmoon version: 3.8
Byte order: little-endian
```

*This is pretty incomplete, there are a number of unknown bytes still.*

*Most of this was put together by reading the RLE Editor source code written
by Zach, which was apparently based in part on the RLE Viewer by Magesturm.*

RLE files store sprites. They are actually defined in LST files but LST files
do not store actual image data; instead they contain references to locations
in RLE files where the actual resource data is located.

Sprites do not have a constant size, even among frames of the same animation.
Presumably in order to save space, each sprite is cropped down from its full
size to avoid storing some transparent pixels. A corresponding offset is 
stored along with the sprite, which can be used to reverse the cropping.


##Header

```
+0
string file_type - a null-terminated ASCII string with the value 
                   "Resource File"

+14
uint   unknown   - not sure what this is

+18
uint   count     - the total number of resources in this file
```

Next are a series of `count` uints, one for each resource. Each uint gives
the starting offset from the beginning of the file to the start of the
resource data.

```
+22+(4*n)
uint   start(n)  - offset to the start of resource (n).
```

##Resource Data

Each resource begins at a unique offset `start(n)` and has the following 
format:

```
RESOURCE+0
uint   length    - total number of bytes in this resource minus 4 (this 4-byte
                   length field is not included in the total).

RESOURCE+4
uint   offset_x  - horizontal image offset

RESOURCE+8
uint   offset_y  - vertical image offset

RESOURCE+12
uint   width     - the width of the image in pixels

RESOURCE+16
uint   height    - the height of the image in pixels

RESOURCE+20
16b    unknown   - 16 unknown bytes. RLE Editor skips them. Looks like 4 uints?
```

Next are (`length` - 32) bytes of image data. Each entry in this section starts
with a byte indicating the nature of the data to follow. There are three
possibilities.

####1. Paint 'total' pixels

When a "1" is read from the first byte of an entry, a 4-byte uint will follow
giving the total number of pixels to paint. Next will follow 2 bytes for each
pixel, each representing the 16-bit color to use for that pixel.

```
ENTRY+0
byte   entry_type - in this case, "1"

ENTRY+1
uint   total      - the total number of pixels to paint

ENTRY+5+(PIXEL*2)+0
byte   color_0    - the first byte in the 16-bit color

ENTRY+5+(PIXEL*2)+1
byte   color_1    - the second byte in the 16-bit color
```


####2. Skip x pixels

When a "2" is read from the first byte of an entry, a 4-byte uint will follow
giving the number of pixels to skip (fill with black).

```
ENTRY+0
byte   entry_type - in this case, "2"

ENTRY+1
uint   total      - the total number of pixels to skip (paint black)
```


####3. End of image data

When a "3" is read from the first byte of an entry, it means that the image
data is over. There is one more byte that can be read, which seems to have
something to do with how much of the image is left to fill with black.

```
ENTRY+0
byte   entry_type - in this case, "3"

ENTRY+1
byte   unknown    - not sure yet, can be at least 0, 1, 2, or 3.
```
