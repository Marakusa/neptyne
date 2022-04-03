#!/bin/sh

# Call nasm assembler
./linux/nasm -w+all -f elf64 -o "$1.o" "$1.asm"

if test -f "$1.o"; then
    # Call linker
    gcc -m64 "$1.o" -o "$1"
    # Cleanup
    rm "$1.o"
fi
