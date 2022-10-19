#!/bin/sh

# Call nasm assembler
"$4"/vendor/nasm/linux/nasm -w+all -f elf64 -o "$2" "$1"

if test -f "$2"; then
    # Call linker
    ld -s -o "$3" "$2"
fi
