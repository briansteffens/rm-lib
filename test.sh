#!/bin/bash

./build.sh

cp "bin/rm-lib.dll" tests/

echo "Compiling rm-lib.tests.dll.."
dmcs /target:library \
    /out:"tests/rm-lib.tests.dll" \
    -r:"/usr/lib/mono/4.5/nunit.framework.dll" \
    -r:"bin/rm-lib.dll" \
    "tests/MessageCtfTests.cs" \
    "tests/ItemsTests.cs" \
    "tests/SpawnTests.cs"

#echo "Press any key to run tests."
#read

cd tests
nunit-console4 "rm-lib.tests.dll"
cd ..
