
; int strlen(byte* str)
strlen:
	push bp
	mov bp, sp
	push r1
	
	mov r1, [bp + 8]
@@:
	cmp byte [r1], 0
	jz @f
	inc r1
	jmp @b
@@:
	mov r0, r1
	sub r0, [bp + 8]
	
.return:
	pop r1
	pop bp
	retn 4

; void strreverse(byte* str)
strreverse:
	push bp
	mov bp, sp
	push r1
	push r2
	push r3
	
	mov r1, [bp + 8]
	mov r2, r1
	invoke strlen, r1
	add r2, r0
	dec r2
	
@@:
	cmp r1, r2
	jae .return
	mov r3, byte [r1]
	mov byte [r1], byte [r2]
	inc r1
	mov byte [r2], r3
	dec r2
	jmp @b

.return:
	pop r3
	pop r2
	pop r1
	pop bp
	retn 4

; void itoa(int value, byte* str)
itoa:
	push bp
	mov bp, sp
	push r1
	push r2
	push r3
	
	mov r1, [bp + 8]  ; value
	mov r2, [bp + 12] ; str
	push r2
	
	cmp r1, 0
	jae .noNegative
	
	pop r2
	mov byte [r2], byte '-'
	inc r2
	push r2
	
.noNegative:
@@:
	mov r3, r1
	rem r3, 10
	abs r3
	mov byte [r2], byte [r3 + .lookup]
	inc r2
	div r1, 10
	cmp r1, r1
	jnz @b
	
	xor byte [r2], byte [r2]
	
	pop r2
	invoke strreverse, r2
	
	jmp .return
	
.return:
	pop r3
	pop r2
	pop r1
	pop bp
	retn 8
	
.lookup:
	db '0123456789'

