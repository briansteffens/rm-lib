#!/bin/bash

mkdir -p ../bin

echo "Compiling rm-lib.dll.."
mcs -sdk:4 -debug \
    /define:DEBUG \
    /target:library \
    /r:"/usr/lib/mono/4.0-api/System.Drawing.dll" \
    /out:"bin/rm-lib.dll" \
    "src/Common.cs" \
    "src/Items.cs" \
    "src/List.cs" \
    "src/MessageCtf.cs" \
    "src/Sprites.cs" \
    "src/Spawn.cs" \
    "src/Skills.cs"
