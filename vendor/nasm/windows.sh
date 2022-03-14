#!/bin/sh
wine win64/nasm.exe -f win32 test.asm -o test.obj
gcc test.obj -o test.exe
./test.exe
