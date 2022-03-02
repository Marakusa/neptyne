%use masm

section .data

section .text
    global _start

_start:
    call main
    mov eax, 1
    mov ebx, 0
    int 80h

