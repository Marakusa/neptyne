section .data

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
	mov eax, 33
	mov DWORD [rbp-4], eax
	mov eax, DWORD [rbp-4]
	mov DWORD [rbp-8], eax
	mov eax, DWORD [rbp-8]
	pop rbp
	ret 
