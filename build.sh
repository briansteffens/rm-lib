#!/bin/bash

mkdir -p ../bin

echo "Compiling rm-lib.dll.."
dmcs -sdk:4 -debug \
    /target:library \
    /r:"/usr/lib/mono/4.0-api/System.Drawing.dll" \
    /out:"bin/rm-lib.dll" \
    "src/Common.cs" \
    "src/MessageCtf.cs" \
    "src/Items.cs" \
    "src/Spawn.cs" \
    "src/RLE.cs"
