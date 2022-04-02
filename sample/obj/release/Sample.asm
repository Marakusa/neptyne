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
	mov eax, 112
	mov DWORD [rbp-4], eax
	mov eax, DWORD [rbp-4]
	mov DWORD [rbp-8], eax
	mov eax, DWORD [rbp-8]
	mov ecx, eax
	mov eax, 4
	mov ebx, 1
	mov edx, 1
	int 80h
	mov eax, DWORD [rbp-8]
	pop rbp
	ret 
