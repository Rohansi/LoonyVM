include 'loonyvm.inc'

halt:
	rand r0
	invoke itoa, r0, itoaBuffer1
	invoke strlen, itoaBuffer1
	invoke puts, str1
	invoke puts, itoaBuffer1
	invoke puts, str2
	invoke strlen, itoaBuffer1
	invoke itoa, r0, itoaBuffer2
	invoke puts, itoaBuffer2
	invoke putc, 32 ; space
	jmp halt

str1: db 'strlen("', 0
str2: db '") = ', 0
itoaBuffer1: db '-1234567890', 0
itoaBuffer2: db '-1234567890', 0

; void putc(byte c)
putc:
	push bp
	mov bp, sp
	push r0
	push r1
	push r2
	push r3

	mov r0, byte [bp + 8]
	mov r1, [_termX]
	mov r2, [_termY]

.backspaceCheck:
	cmp r0, 8 ; \b
	jne .xCheck
	dec r1
	cmp r1, 0
	jae .backspaceClear
.backspaceUpLine:
	mov r1, termSizeX - 1
	dec r2
	cmp r2, 0
	jae .backspaceClear
	xor r1, r1
	xor r2, r2
.backspaceClear:
	mov r3, r2         ; ptr = termAddr + ((y * termSizeX) + x) * 2
	mul r3, termSizeX
	add r3, r1
	mul r3, 2
	add r3, termAddr
	mov word [r3], 0
	jmp .end
.xCheck:
	cmp r1, termSizeX
	jb .yCheck
	xor r1, r1
	inc r2
.yCheck:
	cmp r2, termSizeY
	jb .newlineCheck
	invoke scroll
	dec r2
.newlineCheck:
	cmp r0, 10 ; \n
	jne .write
	xor r1, r1
	inc r2
	cmp r2, termSizeY
	jb .end
	invoke scroll
	dec r2
	jmp .end
.write:
	mov r3, r2         ; ptr = termAddr + ((y * termSizeX) + x) * 2
	mul r3, termSizeX
	add r3, r1
	mul r3, 2
	add r3, termAddr
	mov byte [r3], r0
	mov byte [r3 + 1], 0x0F ; white on black
	inc r1
.end:
	mov [_termX], r1
	mov [_termY], r2

.return:
	pop r3
	pop r2
	pop r1
	pop r0
	pop bp
	retn 4

; void puts(byte* str)
puts:
	push bp
	mov bp, sp
	push r0

	mov r0, [bp + 8]
@@:
	cmp byte [r0], 0
	jz .return
	invoke putc, byte [r0]
	inc r0
	jmp @b

.return:
	pop r0
	pop bp
	retn 4

; void scroll()
scroll:
	push bp
	mov bp, sp
	push r0
	push r1
	push r3

	; workaround for loonyvm.inc bug
	.src = termAddr + (termSizeX * 2)
	.dst = termAddr
	.cnt = termSizeX * (termSizeY - 1)

	mov r0, .src
	mov r1, .dst
	mov r3, .cnt

@@:
	mov word [r1], word [r0]
	add r0, 2
	add r1, 2
	dec r3
	jnz @b

	.dst = termAddr + ((termSizeY - 1) * termSizeX * 2)
	.cnt = termSizeX
	mov r0, .dst
	mov r3, .cnt
@@:
	mov word [r0], 0
	add r0, 2
	dec r3
	jnz @b

.return:
	pop r3
	pop r1
	pop r0
	pop bp
	ret

; void clear()
clear:
	push bp
	mov bp, sp
	push r0
	push r1

	.cnt = termSizeX * termSizeY
	mov r0, termAddr
	mov r1, .cnt

@@:
	mov word [r0], 0
	add r0, 2
	dec r1
	jnz @b

	mov [_termX], 0
	mov [_termY], 0

.return:
	pop r1
	pop r0
	pop bp
	ret
	
_termX: dd 0
_termY: dd 0

termAddr = 0x60000
termSizeX = 80
termSizeY = 25

include 'string.inc'
