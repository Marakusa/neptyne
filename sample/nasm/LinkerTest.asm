extern s_0
extern s_0_len
extern newline

section .data
	s_2 db "This is a test", 0
	s_2_len equ $ - s_2

section .text
	global _linkedTest

_linkedTest:
	push rbp
	mov rbp, rsp
	mov eax, s_2
	mov edx, s_2_len
	mov ecx, eax
	mov eax, 4
	mov ebx, 1
	int 80h

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
