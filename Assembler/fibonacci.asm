include 'loonyvm.inc'

ccall fibonacci, 20
int r0

halt: jmp halt

fibonacci:
    push bp
    mov bp, sp
    
.exitCondition:
    cmp [bp + 8], 2
    jae .recursiveCondition
    mov r0, [bp + 8]
    jmp .return
    
.recursiveCondition:
    push r1
    push r2
    xor r1, r1
    mov r2, [bp + 8]
    
    dec r2
    ccall fibonacci, r2
    mov r1, r0
    
    dec r2
    ccall fibonacci, r2
    add r1, r0
    
    mov r0, r1
    pop r2
    pop r1
    
.return:
    pop bp
    ret

