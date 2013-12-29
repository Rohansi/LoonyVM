

; print a character to the terminal
; void printChar(byte c)
printChar:
    push bp
    mov bp, sp
    push r0
    push r1
    push r2
    push r3

    mov r0, byte [bp + 8]
    mov r1, [termX]
    mov r2, [termY]

.backspaceCheck:
    cmp r0, 8 ; \b
    jne .xCheck
    dec r1
    cmp r1, 0
    jae .backspaceClear
.backspaceUpLine:
    .exp = termSizeX - 1
    mov r1, .exp
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
    invoke termScroll
    dec r2
.newlineCheck:
    cmp r0, 10 ; \n
    jne .write
    xor r1, r1
    inc r2
    cmp r2, termSizeY
    jb .end
    invoke termScroll
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
    mov [termX], r1
    mov [termY], r2

.return:
    pop r3
    pop r2
    pop r1
    pop r0
    pop bp
    retn 4

; print a string to the terminal
; void printString(byte* str)
printString:
    push bp
    mov bp, sp
    push r0

    mov r0, [bp + 8]
@@:
    cmp byte [r0], 0
    jz .return
    invoke printChar, byte [r0]
    inc r0
    jmp @b

.return:
    pop r0
    pop bp
    retn 4

; scroll the terminal up one line
; void scroll()
termScroll:
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

; clear the terminal
; void clear()
termClear:
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

    mov [termX], 0
    mov [termY], 0

.return:
    pop r1
    pop r0
    pop bp
    ret
    
termX: dd 0
termY: dd 0

termAddr = 0x60000
termSizeX = 80
termSizeY = 25
