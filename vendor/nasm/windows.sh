#!/bin/sh
nasm -f win32 test.asm -o test.obj
gcc test.obj -o test.exe
./test.exe
