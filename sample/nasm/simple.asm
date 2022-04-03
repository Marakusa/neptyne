extern printf

section .data
    decimal: db "%d"

section .text
	global _start

_start:
	call _main
	mov eax, 1
	mov ebx, 0
	int 80h

_main:
	push rbp
	mov rbp, rsp

    ; int a = 119;
	mov DWORD [rbp-4], 119

    ; out(a);
	mov eax, DWORD [rbp-4]
	mov esi, eax
	mov edi, decimal
	mov eax, 0
	call printf

	mov eax, 0
	pop rbp
	ret
