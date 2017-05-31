RMM File Format
===============

```
Redmoon client file: DATAs/Map/Map*.rmm
From Redmoon version: 3.8
Byte order: little-endian
```

## Header

```
+0x00
file_type_len: u8   - Length of the file type string in bytes.

+0x01+len
file_type: string   - A length -prefixed string identifier for the file.
                      Should contain : "RedMoon MapData 1.0".
+0x14
x_dimension: u32    - Number of map tiles in `x` dimension.

+0x18
y_dimension: u32    - Number of map tiles in `y` dimension.

+0x1C
count(n): u8        - Number of entries for unknown array.

+0x1D..             - An unknown array. I almost want to say they are pars of
                      4-bit values for tile offsets, being that the `tle*.rle`
                      images are arranged in arrays of (48x24) pixel tiles;
                      none of which seem to go above 16x16 tiles.

+0x1D + (n)
map_num: u16        - The number of the map. This should correspond to the
                      filename.
            
+0x1F + (n)
tileset_1: u16
+0x21 + (n)
tileset_2: u16 
+0x23 + (n)
tileset_3: u16      - Up-to three tile set may used for a map, this is 
                      the number of the `tle*.rle` used. (I think...) 
```


## Tile Data

The tile data is currently somewhat unknown. Each tile consists of 8 bytes.
