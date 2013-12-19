include 'loonyvm.inc'

ccall puts, str1
ccall puts, testStr
ccall puts, str2
ccall strlen, testStr
ccall itoa, r0, itoaBuffer
ccall puts, itoaBuffer
int r0

halt: jmp halt

str1: db 'strlen("', 0
str2: db '") = ', 0
testStr: db 'hello world', 0
itoaBuffer: db '-1234567890', 0

; void puts(byte* str)
puts:
	push bp
	mov bp, sp
	push r1
	push r2
	
	mov r1, [bp + 8]
	mov r2, [cursor]
	
@@:
	cmp byte [r1], 0
	jz .return
	mov byte [r2], byte [r1]
	inc r2
	mov byte [r2], 0x0F
	inc r2
	inc r1
	jmp @b

.return:
	mov [cursor], r2
	pop r2
	pop r1
	pop bp
	ret
	
cursor: dd 0x60000

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
	ret

; void strreverse(byte* str)
strreverse:
	push bp
	mov bp, sp
	push r1
	push r2
	push r3
	
	mov r1, [bp + 8]
	mov r2, r1
	ccall strlen, r1
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
	ret

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
	ccall strreverse, r2
	
	jmp .return
	
.return:
	pop r3
	pop r2
	pop r1
	pop bp
	ret
	
.lookup:
	db '0123456789'

