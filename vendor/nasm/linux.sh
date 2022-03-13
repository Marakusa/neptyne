#!/bin/sh

# Call nasm assembler
./linux/nasm -w+all -f elf64 -o test.o test.asm

# Call linker
ld -o test test.o

./test

# Remove object files and executables
rm test.o
rm test

