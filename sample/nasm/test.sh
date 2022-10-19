#!/bin/sh

linux/nasm -w+all -f elf64 -o LinkerTest.o LinkerTest.asm
linux/nasm -w+all -f elf64 -o Main.o Main.asm

ld -s -o Sample LinkerTest.o Main.o -no-pie
