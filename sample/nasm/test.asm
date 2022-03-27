section .data
	_0x0068656c6c6f: 	db 'This is a test message and this is kinda long because I have to test if this works right...',10,0
	_0x0068656c6c4a: 	db 'Another message',10,0

section .text
	global 	_start

_start:
	call	_0x006d61696e
	mov		rax, 1					; syscall = 1 == exit
	mov		rbx, 0					; fd = 0 == stdin
	int		80h						; Call kernel

_strlen:
	push	rcx
  	xor		rcx, rcx

_strlen_next:
	cmp		[rdi], byte 0  			; null byte yet?
	jz		_strlen_null   			; yes, get out

	inc		rcx            			; char is ok, count it
	inc		rdi            			; move to next char
	jmp		_strlen_next   			; process again

_strlen_null:
	mov		rax, rcx       			; rcx = the length (put in rax)
	pop		rcx            			; restore rcx
	ret                    			; get out

_print:
	push	rbp						; Save address of previous stack frame
	mov		rbp, rsp				; Address of current stack frame

	mov 	rcx, rdi				; The string to write

	call  	_strlen					; Call string length
	mov   	rdx, rax				; Move string length to rdx register

	mov 	rax, 4					; syscall 4 == write
	mov 	rbx, 1					; fd = 1 == stdout

	int 	80h						; Call kernel
	
	pop		rbp						; Cleanup the stack
	ret								; Return

_0x006d61696e:
	push	rbp						; Save address of previous stack frame
	mov		rbp, rsp				; Address of current stack frame
	
    mov     rdi, _0x0068656c6c6f	; Set a function argument
    call    _print					; Call print function
	
    mov     rdi, _0x0068656c6c4a	; Set a function argument
    call    _print					; Call print function
        
    mov     rax, 0					; Set return value from this function to 0
	pop		rbp						; Cleanup the stack
	ret								; Return
