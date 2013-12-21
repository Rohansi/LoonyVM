
; void memset(void* ptr, byte value, int num)
memset:
    push bp
    mov bp, sp
    push r1
    push r2
    push r3

    mov r1, [bp + 8]       ; ptr
    mov r2, byte [bp + 12] ; value
    mov r3, [bp + 16]      ; num

@@:
    cmp r3, r3
    jz .return
    dec r3
    mov byte [r1], r2
    inc r1
    jmp @b

.return:
    pop r3
    pop r2
    pop r1
    pop bp
    retn 12

; void memcpy(void* dst, void* stc, int num)
memcpy:
    push bp
    mov bp, sp
    push r1
    push r2
    push r3

    mov r1, [bp + 8]  ; dst
    mov r2, [bp + 12] ; src
    mov r3, [bp + 16] ; num

@@:
    cmp r3, r3
    jz .return
    dec r3
    mov byte [r1], byte [r2]
    inc r1
    inc r2
    jmp @b

.return:
    pop r3
    pop r2
    pop r1
    pop bp
    retn 12

; void memmove(void* dst, void* src, int num)
memmove:
    push bp
    mov bp, sp
    push r1
    push r2
    push r3

    mov r1, [bp + 8]  ; dst
    mov r2, [bp + 12] ; src
    mov r3, [bp + 16] ; num

    cmp r2, r1
    jb .rev
@@:
    cmp r3, r3
    jz .return
    dec r3
    mov byte [r1], byte [r2]
    inc r1
    inc r2
    jmp @b

.rev:
    add r1, r3
    dec r1
    add r2, r3
    dec r2
@@:
    cmp r3, r3
    jz .return
    dec r3
    mov byte [r1], byte [r2]
    dec r1
    dec r2
    jmp @b

.return:
    pop r3
    pop r2
    pop r1
    pop bp
    retn 12

; returns the length of a string
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

; copies string from src into dst
; void strcpy(byte* dst, byte* src)
strcpy:
    push bp
    mov bp, sp
    push r1
    push r2

    mov r1, [bp + 8]  ; dst
    mov r2, [bp + 12] ; src

@@:
    cmp byte [r2], byte [r2]
    jz .done
    mov byte [r1], byte [r2]
    inc r1
    inc r2
    jmp @b

.done:
    xor byte [r1], byte [r1]

.return:
    pop r2
    pop r1
    pop bp
    retn 8

; void strcat(byte* dst, byte* src)
strcat:
    push bp
    mov bp, sp
    push r0
    push r1
    push r2
    push r3

    mov r1, [bp + 8]  ; dst
    mov r2, [bp + 12] ; src

    invoke strlen, r1
    add r1, r0

@@:
    cmp byte [r2], byte [r2]
    jz .done
    mov byte [r1], byte [r2]
    inc r1
    inc r2
    jmp @b

.done:
    xor byte [r1], byte [r1]

.return:
    pop r3
    pop r2
    pop r1
    pop r0
    pop bp
    retn 8

; int strcmp(byte* str1, byte* str2)
strcmp:
    push bp
    mov bp, sp
    push r1
    push r2

    mov r1, [bp + 8]  ; str1
    mov r2, [bp + 12] ; str2

@@:
    cmp byte [r1], byte [r2]
    jne .done
    cmp byte [r1], byte [r1]
    jnz .notEqual
    xor r0, r0
    jmp .return
.notEqual:
    inc r1
    inc r2
    jmp @b

.done:
    mov r0, byte [r1]
    sub r0, byte [r2]

.return:
    pop r2
    pop r1
    pop bp
    retn 8

; reverses a string
; void strreverse(byte* str)
strreverse:
    push bp
    mov bp, sp
    push r0
    push r1
    push r2
    
    mov r1, [bp + 8]
    mov r2, r1
    invoke strlen, r1
    add r2, r0
    dec r2
    
@@:
    cmp r1, r2
    jae .return
    xchg byte [r1], byte [r2]
    inc r1
    dec r2
    jmp @b

.return:
    pop r2
    pop r1
    pop r0
    pop bp
    retn 4

; converts an integer to a string
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
    mov byte [r2], byte [r3 + itoaLookup]
    inc r2
    div r1, 10
    cmp r1, r1
    jnz @b
    
    xor byte [r2], byte [r2]
    
    pop r2
    invoke strreverse, r2
    
.return:
    pop r3
    pop r2
    pop r1
    pop bp
    retn 8
    

; converts pointer to a hex string
; void ptoa(void* ptr, byte* str)
ptoa:
    push bp
    mov bp, sp
    push r0
    push r1
    push r2

    mov r0, [bp + 8]  ; ptr
    mov r1, [bp + 12] ; str
    mov r2, 8
    push r1

@@:
    push r0
    and r0, 0xF
    mov byte [r1], byte [r0 + itoaLookup]
    inc r1
    pop r0
    shr r0, 4
    dec r2
    jnz @b

    xor byte [r1], byte [r1]
    pop r1
    invoke strreverse, r1

.return:
    pop r2
    pop r1
    pop r0
    pop bp
    retn 8

itoaLookup:
    db '0123456789ABCDEF'

