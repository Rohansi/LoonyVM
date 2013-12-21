
; disassemble the instruction at addr into str
; str should at least be 75 bytes long
; returns the length of the instruction (not the string) or 0 on error 
;
; int disassemble(void* addr, byte* str)
disassemble:
    push bp
    mov bp, sp
    push r1
    push r2
    push r3
    push r4
    push r5
    push r6
    push r7

    mov r1, [bp + 12]  ; str
    ;   r2             ; operandCount
    ;   r3             ; writeSrc
    ;   r4             ; temp
    ;   r5             ; ptr
    mov r6, 4          ; payload offset
    mov r7, [bp + 8]   ; addr

    invoke ptoa, r7, r1
    add r1, 8
    
    mov word [r1], ': '
    add r1, 2

    mov r2, [r7]
    and r2, 0xFF
    cmp r2, disasmInstrCount
    ja .error

    push r2
    mul r2, 4 ; sizeof(dword)
    mov r3, [r2 + disasmInstrLookup]
    call .write
    pop r2

    mov r2, byte [r2 + disasmOperandLookup]
    cmp r2, 0
    je .done

    mov byte [r1], ' '
    inc r1

    mov r4, [r7 + 3]
    and r4, 00110000b
    shr r4, 4
    cmp r4, disasmTypeCount
    ja .error
    mul r4, 4 ; sizeof(dword)
    mov r3, [r4 + disasmTypeLookup]
    call .write

    mov r5, [r7 + 3]
    and r5, 10000000b

    cmp r5, r5
    jz @f
    mov byte [r1], '['
    inc r1
@@:

    mov r4, [r7 + 3]
    and r4, 01000000b
    jz @f
    mov r4, [r7 + 2]
    and r4, 0xF0
    shr r4, 4
    cmp r4, disasmRegCount
    ja .error
    mul r4, 4 ; sizeof(dword)
    mov r3, [r4 + disasmRegLookup]
    call .write
    mov r3, disasmPlus
    call .write

@@:
    mov r4, [r7 + 1]
    and r4, 0xF0
    shr r4, 4
    cmp r4, disasmRegCount
    ja .lopImm
    mul r4, 4 ; sizeof(dword)
    mov r3, [r4 + disasmRegLookup]
    call .write
    jmp @f

.lopImm:
    cmp r4, 0x0D
    jne .lopWord
    mov byte r4, byte [r7 + r6]
    inc r6 ; sizeof(byte)
    invoke itoa, byte r4, disasmItoa
    mov r3, disasmItoa
    call .write
    jmp @f
.lopWord:
    cmp r4, 0x0E
    jne .lopDword
    mov word r4, word [r7 + r6]
    add r6, 2 ; sizeof(word)
    invoke itoa, word r4, disasmItoa
    mov r3, disasmItoa
    call .write
    jmp @f
.lopDword:
    mov r4, [r7 + r6]
    add r6, 4 ; sizeof(dword)
    invoke itoa, r4, disasmItoa
    mov r3, disasmItoa
    call .write

@@:
    cmp r5, r5
    jz @f
    mov byte [r1], ']'
    inc r1

; -- SECOND OPERAND -- ;
@@:
    cmp r2, 1
    je .done

    mov word [r1], ', '
    add r1, 2

    mov r4, [r7 + 3]
    and r4, 00000011b
    cmp r4, disasmTypeCount
    ja .error
    mul r4, 4 ; sizeof(dword)
    mov r3, [r4 + disasmTypeLookup]
    call .write

    mov r5, [r7 + 3]
    and r5, 00001000b

    cmp r5, r5
    jz @f
    mov byte [r1], '['
    inc r1
@@:

    mov r4, [r7 + 3]
    and r4, 00000100b
    jz @f
    mov r4, [r7 + 2]
    and r4, 0x0F
    cmp r4, disasmRegCount
    ja .error
    mul r4, 4 ; sizeof(dword)
    mov r3, [r4 + disasmRegLookup]
    call .write
    mov r3, disasmPlus
    call .write

@@:
    mov r4, [r7 + 1]
    and r4, 0x0F
    cmp r4, disasmRegCount
    ja .ropImm
    mul r4, 4 ; sizeof(dword)
    mov r3, [r4 + disasmRegLookup]
    call .write
    jmp @f

.ropImm:
    cmp r4, 0x0D
    jne .ropWord
    mov byte r4, byte [r7 + r6]
    inc r6 ; sizeof(byte)
    invoke itoa, byte r4, disasmItoa
    mov r3, disasmItoa
    call .write
    jmp @f
.ropWord:
    cmp r4, 0x0E
    jne .ropDword
    mov word r4, word [r7 + r6]
    add r6, 2 ; sizeof(word)
    invoke itoa, word r4, disasmItoa
    mov r3, disasmItoa
    call .write
    jmp @f
.ropDword:
    mov r4, [r7 + r6]
    add r6, 4 ; sizeof(dword)
    invoke itoa, r4, disasmItoa
    mov r3, disasmItoa
    call .write

@@:
    cmp r5, r5
    jz .done
    mov byte [r1], ']'
    inc r1

.done:
    xor byte [r1], byte [r1] ; null terminator

    mov r0, r6
    jmp .return

; mini-function to write output strings
; call with r3 set to the string addr
.write:
    @@:
        cmp byte [r3], 0
        jz @f
        mov byte [r1], byte [r3]
        inc r1
        inc r3
        jmp @b
    @@:
    ret

.error:
    xor r0, r0

.return:
    pop r7
    pop r6
    pop r5
    pop r4
    pop r3
    pop r2
    pop r1
    pop bp
    retn 8

disasmPlus:  db ' + ', 0
disasmItoa:  db '-1234567890', 0

; ==========================================

disasmInstrLookup:
    dd disasmMov       ; 00
    dd disasmAdd       ; 01
    dd disasmSub       ; 02
    dd disasmMul       ; 03
    dd disasmDiv       ; 04
    dd disasmRem       ; 05
    dd disasmInc       ; 06
    dd disasmDec       ; 07
    dd disasmNot       ; 08
    dd disasmAnd       ; 09
    dd disasmOr        ; 0A
    dd disasmXor       ; 0B
    dd disasmShl       ; 0C
    dd disasmShr       ; 0D
    dd disasmPush      ; 0E
    dd disasmPop       ; 0F
    dd disasmJmp       ; 10
    dd disasmCall      ; 11
    dd disasmRet       ; 12
    dd disasmCmp       ; 13
    dd disasmJz        ; 14
    dd disasmJnz       ; 15
    dd disasmJe        ; 16
    dd disasmJne       ; 17
    dd disasmJa        ; 18
    dd disasmJae       ; 19
    dd disasmJb        ; 1A
    dd disasmJbe       ; 1B
    dd disasmRand      ; 1C
    dd disasmInt       ; 1D
    dd disasmIret      ; 1E
    dd disasmIvt       ; 1F
    dd disasmAbs       ; 20
    dd disasmRetn      ; 21
    dd disasmXchg      ; 22
    dd disasmCmpxchg   ; 23
    dd disasmPusha     ; 24
    dd disasmPopa      ; 25
    dd disasmSti       ; 26
    dd disasmCli       ; 27
disasmInstrCount     = 0x27

disasmRegLookup:
    dd disasmR0        ; 00
    dd disasmR1        ; 01
    dd disasmR2        ; 02
    dd disasmR3        ; 03
    dd disasmR4        ; 04
    dd disasmR5        ; 05
    dd disasmR6        ; 06
    dd disasmR7        ; 07
    dd disasmR8        ; 08
    dd disasmR9        ; 09
    dd disasmBP        ; 0A
    dd disasmIP        ; 0B
    dd disasmSP        ; 0C
disasmRegCount       = 0x0C

disasmTypeLookup:
    dd disasmByte      ; 00
    dd disasmWord      ; 01
    dd disasmDword     ; 02
disasmTypeCount      = 0x02

disasmOperandLookup:
    db 2               ; mov
    db 2               ; add
    db 2               ; sub
    db 2               ; mul
    db 2               ; div
    db 2               ; rem
    db 1               ; inc
    db 1               ; dec
    db 1               ; not
    db 2               ; and
    db 2               ; or
    db 2               ; xor
    db 2               ; shl
    db 2               ; shr
    db 1               ; push
    db 1               ; pop
    db 1               ; jmp
    db 1               ; call
    db 0               ; ret
    db 2               ; cmp
    db 1               ; jz
    db 1               ; jnz
    db 1               ; je
    db 1               ; jne
    db 1               ; ja
    db 1               ; jae
    db 1               ; jb
    db 1               ; jbe
    db 1               ; rand
    db 1               ; int
    db 0               ; iret
    db 1               ; ivt
    db 1               ; abs
    db 1               ; retn
    db 2               ; xchg
    db 2               ; cmpxchg
    db 0               ; pusha
    db 0               ; popa
    db 0               ; sti
    db 0               ; cli

disasmMov:        db 'mov', 0
disasmAdd:        db 'add', 0
disasmSub:        db 'sub', 0
disasmMul:        db 'mul', 0
disasmDiv:        db 'div', 0
disasmRem:        db 'rem', 0
disasmInc:        db 'inc', 0
disasmDec:        db 'dec', 0
disasmNot:        db 'not', 0
disasmAnd:        db 'and', 0
disasmOr:         db 'or', 0
disasmXor:        db 'xor', 0
disasmShl:        db 'shl', 0
disasmShr:        db 'shr', 0
disasmPush:       db 'push', 0
disasmPop:        db 'pop', 0
disasmJmp:        db 'jmp', 0
disasmCall:       db 'call', 0
disasmRet:        db 'ret', 0
disasmCmp:        db 'cmp', 0
disasmJz:         db 'jz', 0
disasmJnz:        db 'jnz', 0
disasmJe:         db 'je', 0
disasmJne:        db 'jne', 0
disasmJa:         db 'ja', 0
disasmJae:        db 'jae', 0
disasmJb:         db 'jb', 0
disasmJbe:        db 'jbe', 0
disasmRand:       db 'rand', 0
disasmInt:        db 'int', 0
disasmIret:       db 'iret', 0
disasmIvt:        db 'ivt', 0
disasmAbs:        db 'abs', 0
disasmRetn:       db 'retm', 0
disasmXchg:       db 'xchg', 0
disasmCmpxchg:    db 'cmpxchg', 0
disasmPusha:      db 'pusha', 0
disasmPopa:       db 'popa', 0
disasmSti:        db 'sti', 0
disasmCli:        db 'cli', 0

disasmR0:         db 'r0', 0
disasmR1:         db 'r1', 0
disasmR2:         db 'r2', 0
disasmR3:         db 'r3', 0
disasmR4:         db 'r4', 0
disasmR5:         db 'r5', 0
disasmR6:         db 'r6', 0
disasmR7:         db 'r7', 0
disasmR8:         db 'r8', 0
disasmR9:         db 'r9', 0
disasmBP:         db 'bp', 0
disasmIP:         db 'ip', 0
disasmSP:         db 'sp', 0

disasmByte:       db 'byte ', 0
disasmWord:       db 'word ', 0
disasmDword:      db 0 ; dword is default
