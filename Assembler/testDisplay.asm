include 'loonyvm.inc'

; laziest display demo
; write all colors in the palette endlessly

; switch to graphics display
mov r0, 0
mov r1, 1
int 6

xor r3, r3

draw:
	.cnt = screenSizeX * screenSizeY
	mov r1, .cnt
	mov r2, screenAddr

@@:
	cmp r1, r1
	jz draw
	mov byte [r2], byte r3
	dec r1
	inc r2
	inc r3
	rem r3, 255
	jmp @b

screenAddr = 0x60000
screenSizeX = 320
screenSizeY = 200
