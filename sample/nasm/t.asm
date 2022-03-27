section .data

section .text
	global _start

_start:
	call _main
	mov ebx, eax
	mov eax, 1
	int 80h

_main:
	push rbp
	mov rbp, rsp
	mov DWORD [rbp-1], 33
	mov DWORD [rbp-5], 12
	mov eax, DWORD [rbp-1]
	pop rbp
	ret
