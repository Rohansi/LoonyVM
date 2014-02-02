include 'loonyvm.inc'

VIDEO_ADDR      = 0x60000
VIDEO_WIDTH     = 320
VIDEO_HEIGHT    = 200

; switch to graphics mode
inc r1
int 6

main:
    xor r3, r3
.draw:
    xor r1, r1
    add r3, 2
@@:
    cmp r1, VIDEO_WIDTH * VIDEO_HEIGHT
    je .draw

    mov r4, r1
    rem r4, VIDEO_WIDTH ; x = i % width
    mov r5, r1
    div r5, VIDEO_WIDTH ; y = i / width

    add r4, r3
    add r5, r3
    xor r4, r5

    mov byte [r1 + VIDEO_ADDR], r4
    inc r1
    jmp @b

;inc r1
;int 6
;jmp $
;
;rb 0x60000 - ($-$$)
;file 'out.pic'
