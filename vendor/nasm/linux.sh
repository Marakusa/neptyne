#!/bin/sh

# Call nasm assembler
./linux/nasm -w+all -f elf64 -o $1.o $1.asm

if test -f "$1.o"; then
    # Call linker
    ld -s -o $1 $1.o

    if test -f "$1"; then
        ./$1
    fi

    # Remove object files and executables
    #rm $1.o
    #rm $1
fi
