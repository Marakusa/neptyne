section .data
	message db 'The character is: %c', 10, 0

section .text
	extern printf   ; If you need other functions, list them in a similar way
	global _start

_start:
    mov eax, 0x21  ; The '!' character
    push rax
    push message
    call printf
    add esp, 8     ; Restore stack - 4 bytes for eax, and 4 bytes for 'message'
    ret
