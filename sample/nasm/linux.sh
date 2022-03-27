#!/bin/sh

# Call nasm assembler
./linux/nasm -w+all -f elf64 -o "$2/$1.o" "$1.asm"

if test -f "$2/$1.o"; then
    # Call linker
    ld -s -o "$2/$1" "$2/$1.o"
    # Cleanup
    rm "$2/$1.o"
fi
