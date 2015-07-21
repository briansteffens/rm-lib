LST File Format
===============

```
Redmoon client file: ./RLEs/*.lst
                     ./RLEs/Chr/*.lst
From Redmoon version: 3.8
Byte order: little-endian
```

Files of this type relate a resource ID to a resource within an RLE file.
RMD files use these resource IDs to refer to particular sprites and organize
them into animations. So a LST file can be thought of as an interface between
RMD files and RLE files or as a table of contents for a set of RLE files.

LST files can be found in the `RLEs` directory of the Redmoon client. For any
LST file it can be assumed that its contents refer only to RLE files found in 
a folder with the same name as the LST file (but uppercase and minus the file 
extension). For example `./RLEs/Chr/c01.lst` explains all files matching the
pattern `./RLEs/Chr/C01/*.rle`.

LST files contain two major sections: a header and the resource data.


##Header

```
string file_type     - A length-prefixed string giving a human-readable file
                       type. In the files I tested with, this was
                       "RedMoon Lst File".

string file_version  - A length-prefixed string giving the file version number.
                       In the files I tested with, this was "1.0".

uint   next_id       - Not 100% certain on this, but it seems to be a resource
                       ID counter, here as a convenience to editors when adding
                       new entries to this file. In all of the examples I
                       checked, this number was 1 more than the highest resource
                       ID in the file. So when a resource needs to be added, the
                       value in next_id can be used as the new resource ID, and
                       then next_id should be incremented and saved back to the
                       file.

uint   total_entries - The total number of resources defined in this file.
```


##Resource data

The rest of the file contains the following data structure looped 
`total_entries` times, one for each resource defined in this file:

```
string unknown      - Not sure what this is or if it has any significance. It
                      seems too structured and consistent to be a free-form
                      comment field. Some vague possibilities without evidence
                      are: 1) some kind of global resource identifier, maybe
                      using an odd base like base32 or 2) maybe these are the
                      filenames from the original images that were loaded into
                      the RLE files by JCE.

uint   resource_id  - The ID of the resource. This is the value RMD files use
                      to refer to resources. This should be unique within a
                      LST file. They do not need to be sequential or ordered.
                   

uint   rle_file     - The index of the RLE file in which the resource data is
                      stored. For example, if this was set to 5 in a file named
                      "RLEs/bul.lst", this would be referring to the RLE file
                      "RLEs/Bul/bul00005.rle".

uint   index_in_rle - The index of the resource data within the RLE file. This
                      is a positional index.
```
