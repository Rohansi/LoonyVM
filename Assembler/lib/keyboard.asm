
; reads a keyboard event
;  - upper 8 bits are state, lower 8 are scancode
;  - returns -1 if no events are available
; short kbRead()
kbDequeue:
	push bp
	mov bp, sp
	cli

	invoke kbBuffIsEmpty
	cmp r0, r0
	jz .notEmpty
	mov r0, -1
	jmp .return

.notEmpty:
	invoke kbBuffRead

.return:
	sti
	pop bp
	ret

; enqueues a keyboard event
; should only be called by the keyboard irq
; void kbWrite(short event)
kbEnqueue:
	push bp
	mov bp, sp
	push r0
	push r1

	mov r1, word [bp + 8] ; event

	invoke kbBuffIsFull
	cmp r0, r0
	jnz .return ; should beep or something

	invoke kbBuffWrite, r1

	push r2
	mov r2, r1
	and r1, 0xFF
	shr r2, 8
	mov byte [r1 + kbStates], r2
	pop r2

.return:
	pop r1
	pop r0
	pop bp
	retn 4

; check if a key is pressed
; bool kbIsPressed(byte scancode)
kbIsPressed:
	push bp
	mov bp, sp

	mov r0, [bp + 8] ; scancode
	and r0, 0xFF
	mov r0, byte [r0 + kbStates]

.return:
	pop bp
	retn 4

kbStates: rb 256

kbInterruptHandler:
    mov r2, r0
    shl r2, 8
    or r2, r1
    invoke kbEnqueue, word r2
.return:
    iret

; Circular buffer from:
; http://en.wikipedia.org/w/index.php?title=Circular_buffer&oldid=586699125#Always_Keep_One_Slot_Open

; bool kbBuffIsFull()
kbBuffIsFull:
	push bp
	mov bp, sp
	push r1

	xor r0, r0
	mov r1, [kbBuffHead]
	inc r1
	rem r1, kbBuffSize
	cmp r1, [kbBuffTail]
	jne .return
	inc r0

.return:
	pop r1
	pop bp
	ret

; bool kbBuffIsEmpty()
kbBuffIsEmpty:
	push bp
	mov bp, sp

	xor r0, r0
	cmp [kbBuffHead], [kbBuffTail]
	jne .return
	inc r0

.return:
	pop bp
	ret

; void kbBuffWrite(short value)
kbBuffWrite:
	push bp
	mov bp, sp
	push r0
	push r1

	mov r0, word [bp + 8] ; value

	mov r1, [kbBuffHead]
	mul r1, 2
	add r1, kbBuff
	mov word [r1], r0

	mov r1, [kbBuffHead]
	inc r1
	rem r1, kbBuffSize
	mov [kbBuffHead], r1

	cmp [kbBuffHead], [kbBuffTail]
	jne .return

	mov r1, [kbBuffTail]
	inc r1
	rem r1, kbBuffSize
	mov [kbBuffTail], r1

.return:
	pop r1
	pop r0
	pop bp
	retn 4

; short kbBuffRead()
kbBuffRead:
	push bp
	mov bp, sp
	push r1

	mov r0, [kbBuffTail]
	mul r0, 2
	add r0, kbBuff
	mov r0, word [r0]

	mov r1, [kbBuffTail]
	inc r1
	rem r1, kbBuffSize
	mov [kbBuffTail], r1

.return:
	pop r1
	pop bp
	ret

kbBuffSize = 64

kbBuffHead: dd 0 ; write (end)
kbBuffTail: dd 0 ; read (start)
kbBuff:		rw kbBuffSize
