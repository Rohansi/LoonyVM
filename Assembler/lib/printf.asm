; void printf(byte* fmt, ...)
printf:
    push bp
    mov bp, sp
    pusha
    
    mov r2, [bp + 8] ; arg count
    mov r3, bp + 12  ; arg offset
    mov r9, .putc    ; putc func
    
    cmp r2, 1
    jb .return
    
    mov r1, [r3]  ; fmt
    dec r2
    add r3, 4
    
    call printfImpl
    jmp .return
    
.putc:
    invoke putc, byte r8
    ret
    
.return:
    popa
    pop bp
    ret

; void sprintf(byte* str, byte* fmt, ...)
sprintf:
    push bp
    mov bp, sp
    pusha
    
    mov r2, [bp + 8]  ; arg count
    mov r3, bp + 12   ; arg offset
    mov r9, .putc     ; putc func
    
    cmp r2, 2
    jb .return
    
    mov r7, [r3]      ; output str
    mov r1, [r3 + 4]  ; fmt
    sub r2, 2
    add r3, 8
    
    call printfImpl
    jmp .return
    
.putc:
    mov byte [r7], r8
    inc r7
    ret
    
.return:
    popa
    pop bp
    ret

; r1 = fmt
; r2 = arg count
; r3 = arg offset
; r4 = temp
; r7 = reserved
; r8 = reserved
; r9 = putc func (r8 = c)
printfImpl:
    push bp
    mov bp, sp
    sub sp, 16 ; *toa buffers, bp + 8
    
.loop:
    cmp byte [r1], byte [r1]
    jz .return
    
    cmp byte [r1], '%'
    je .fmt
    
.noFmt:
    mov r8, byte [r1]
    call r9
    jmp .cont
    
.fmt:
    inc r1

    cmp r2, 0
    jbe .noMoreArgs

.checkS:
    cmp byte [r1], 's'
    jne .checkC
    
    mov r4, [r3]
    add r3, 4
    dec r2
@@:
    cmp byte [r4], byte [r4]
    jz @f
    mov r8, byte [r4]
    call r9
    inc r4
    jmp @b
@@:
    jmp .cont
    
.checkC:
    cmp byte [r1], 'c'
    jne .checkI
    
    mov r8, byte [r3]
    call r9
    add r3, 4
    dec r2
    jmp .cont
    
.checkI:
    cmp byte [r1], 'i'
    jne .checkP
    
    mov r4, [r3]
    add r3, 4
    dec r2
    invoke itoa, r4, bp + 8
    mov r4, bp + 8
@@:
    cmp byte [r4], byte [r4]
    jz @f
    mov r8, byte [r4]
    call r9
    inc r4
    jmp @b
@@:
    jmp .cont
    
.checkP:
    cmp byte [r1], 'p'
    jne .default
    
    mov r4, [r3]
    add r3, 4
    dec r2
    invoke ptoa, r4, bp + 8
    mov r4, bp + 8
@@:
    cmp byte [r4], byte [r4]
    jz @f
    mov r8, byte [r4]
    call r9
    inc r4
    jmp @b
@@:
    jmp .cont

.noMoreArgs:
.checkEscape:
    cmp byte [r1], '%'
    jne .default

    mov r8, byte '%'
    call r9
    jmp .cont

.default:
    mov r8, byte '%'
    call r9
    mov r8, byte [r1]
    call r9
    
.cont:
    inc r1
    jmp .loop
    
.return:
    add sp, 16
    pop bp
    ret

