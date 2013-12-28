
; void gets(byte* str, int maxLen)
gets:
    push bp
    mov bp, sp
    push r0
    push r1
    push r2
    push r3

    mov r1, [bp + 8]  ; str
    mov r2, [bp + 12] ; maxLen
    xor r3, r3        ; len

    dec r2 ; space for null

.block:
    call getcInternal

.checkEnter:
    cmp r0, 10 ; \n
    jne .checkBackspace

    xor byte [r1 + r3], byte [r1 + r3]
    invoke putc, r0
    jmp .return

.checkBackspace:
    cmp r0, 8 ; \b
    jne .checkMaxLen

    cmp r3, r3
    jz .block
    dec r3
    xor byte [r1 + r3], byte [r1 + r3]
    invoke putc, r0
    jmp .block

.checkMaxLen:
    cmp r3, r2
    jb .checkPrintable

    ; str full, beep or something
    jmp .block

.checkPrintable:
    cmp r0, ' '
    jb .block
    cmp r0, '~'
    ja .block

.append:
    mov byte [r1 + r3], r0
    inc r3
    invoke putc, r0
    jmp .block

.return:
    pop r3
    pop r2
    pop r1
    pop r0
    pop bp
    retn 8

getcInternal:
    push r1
    push r2

.block:
    invoke kbDequeue
    cmp r0, -1
    je .block

    mov r1, r0
    and r1, 0xFF00
    jz .block

    and r0, 0x00FF
    mov r2, r0

    invoke kbIsPressed, 130 ; LShift
    cmp r0, r0
    jnz .translate
    invoke kbIsPressed, 134 ; RShift

.translate:
    mov r1, kbLowerMap
    cmp r0, r0
    jz @f
    mov r1, kbUpperMap
@@:
    mov r0, byte [r1 + r2]

.return:
    pop r2
    pop r1
    ret

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
kbBuff:     rw kbBuffSize

kbLowerMap:
    db   0,   1,   2,   3,   4,   5,   6,   7 ;   0 -   7
    db   8,   9,  10,  11,  12,  13,  14,  15 ;   8 -  15
    db  16,  17,  18,  19,  20,  21,  22,  23 ;  16 -  23
    db  24,  25,  26,  27,  28,  29,  30,  31 ;  24 -  31
    db  32,  33,  34,  35,  36,  37,  38, "'" ;  32 -  39
    db  40,  41,  42,  43, ',', '-', '.', '/' ;  40 -  47
    db '0', '1', '2', '3', '4', '5', '6', '7' ;  48 -  55
    db '8', '9',  58, ';',  60, '=',  62,  63 ;  56 -  63
    db  64,  65,  66,  67,  68,  69,  70,  71 ;  64 -  71
    db  72,  73,  74,  75,  76,  77,  78,  79 ;  72 -  79
    db  80,  81,  82,  83,  84,  85,  86,  87 ;  80 -  87
    db  88,  89,  90, '[', '\', ']',  94,  95 ;  88 -  95
    db '`', 'a', 'b', 'c', 'd', 'e', 'f', 'g' ;  96 - 103
    db 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o' ; 104 - 111
    db 'p', 'q', 'r', 's', 't', 'u', 'v', 'w' ; 112 - 119
    db 'x', 'y', 'z', 123, 124, 125, 126, 127 ; 120 - 127
    db 128, 129, 130, 131, 132, 133, 134, 135 ; 128 - 135
    db 136, 137, 138, 139, 140, 141, 142, 143 ; 136 - 143
    db 144, 145, 146, 147, 148, 149, 150, 151 ; 144 - 151
    db 152, 153, 154, 155, 156, 157, 158, 159 ; 152 - 159
    db '0', '1', '2', '3', '4', '5', '6', '7' ; 160 - 167
    db '8', '9', '+', '-', '*', '/', 174, 175 ; 168 - 175
    db 176, 177, 178, 179, 180, 181, 182, 183 ; 176 - 183
    db 184, 185, 186, 187, 188, 189, 190, 191 ; 184 - 191
    db 192, 193, 194, 195, 196, 197, 198, 199 ; 192 - 199
    db 200, 201, 202, 203, 204, 205, 206, 207 ; 200 - 207
    db 208, 209, 210, 211, 212, 213, 214, 215 ; 208 - 215
    db 216, 217, 218, 219, 220, 221, 222, 223 ; 216 - 223
    db 224, 225, 226, 227, 228, 229, 230, 231 ; 224 - 231
    db 232, 233, 234, 235, 236, 237, 238, 239 ; 232 - 239
    db 240, 241, 242, 243, 244, 245, 246, 247 ; 240 - 247
    db 248, 249, 250, 251, 252, 253, 254, 255 ; 240 - 255

kbUpperMap:
    db   0,   1,   2,   3,   4,   5,   6,   7 ;   0 -   7
    db   8,   9,  10,  11,  12,  13,  14,  15 ;   8 -  15
    db  16,  17,  18,  19,  20,  21,  22,  23 ;  16 -  23
    db  24,  25,  26,  27,  28,  29,  30,  31 ;  24 -  31
    db  32,  33,  34,  35,  36,  37,  38, '"' ;  32 -  39
    db  40,  41,  42,  43, '<', '_', '>', '?' ;  40 -  47
    db ')', '!', '@', '#', '$', '%', '^', '&' ;  48 -  55
    db '*', '(',  58, ':',  60, '+',  62,  63 ;  56 -  63
    db  64,  65,  66,  67,  68,  69,  70,  71 ;  64 -  71
    db  72,  73,  74,  75,  76,  77,  78,  79 ;  72 -  79
    db  80,  81,  82,  83,  84,  85,  86,  87 ;  80 -  87
    db  88,  89,  90, '{', '|', '}',  94,  95 ;  88 -  95
    db '~', 'A', 'B', 'C', 'D', 'E', 'F', 'G' ;  96 - 103
    db 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O' ; 104 - 111
    db 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W' ; 112 - 119
    db 'X', 'Y', 'Z', 123, 124, 125, 126, 127 ; 120 - 127
    db 128, 129, 130, 131, 132, 133, 134, 135 ; 128 - 135
    db 136, 137, 138, 139, 140, 141, 142, 143 ; 136 - 143
    db 144, 145, 146, 147, 148, 149, 150, 151 ; 144 - 151
    db 152, 153, 154, 155, 156, 157, 158, 159 ; 152 - 159
    db '0', '1', '2', '3', '4', '5', '6', '7' ; 160 - 167
    db '8', '9', '+', '-', '*', '/', 174, 175 ; 168 - 175
    db 176, 177, 178, 179, 180, 181, 182, 183 ; 176 - 183
    db 184, 185, 186, 187, 188, 189, 190, 191 ; 184 - 191
    db 192, 193, 194, 195, 196, 197, 198, 199 ; 192 - 199
    db 200, 201, 202, 203, 204, 205, 206, 207 ; 200 - 207
    db 208, 209, 210, 211, 212, 213, 214, 215 ; 208 - 215
    db 216, 217, 218, 219, 220, 221, 222, 223 ; 216 - 223
    db 224, 225, 226, 227, 228, 229, 230, 231 ; 224 - 231
    db 232, 233, 234, 235, 236, 237, 238, 239 ; 232 - 239
    db 240, 241, 242, 243, 244, 245, 246, 247 ; 240 - 247
    db 248, 249, 250, 251, 252, 253, 254, 255 ; 240 - 255
