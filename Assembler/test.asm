include 'loonyvm.inc'

invoke puts, str1
invoke puts, testStr
invoke puts, str2
invoke strlen, testStr
invoke itoa, r0, itoaBuffer
invoke puts, itoaBuffer
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
	retn 4
	
cursor: dd 0x60000

include 'string.inc'
