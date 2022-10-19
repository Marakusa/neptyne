extern _linkedTest

section .data
	newline db 0xa
	s_0 db "Hello, world!", 0
	s_0_len equ $ - s_0

section .text
	global main
	global newline
	global s_0
	global s_0_len

_test:
	push rbp
	mov rbp, rsp
	mov eax, s_0
	mov edx, s_0_len
	mov ecx, eax
	mov eax, 4
	mov ebx, 1
	int 80h

	mov eax, newline
	mov edx, 1
	mov ecx, eax
	mov eax, 4
	mov ebx, 1
	int 80h

	mov eax, 0
	pop rbp
	ret

main:
	push rbp
	mov rbp, rsp
	call _test
	call _linkedTest
	mov eax, 0
	pop rbp
	ret
