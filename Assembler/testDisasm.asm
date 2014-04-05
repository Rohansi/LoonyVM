include 'loonyvm.inc'

ivt interruptTable
sti

; long instruction to play with
;cmpxchg word [r0 + -2147483648], word [r0 + -2147483648]

@@:
    rand byte r0
    rand byte r1
    invoke itoa, byte r0, itoaBuffer
    invoke printString, itoaBuffer
    invoke printString, strDiv
    invoke itoa, byte r1, itoaBuffer
    invoke printString, itoaBuffer
    invoke printString, strEq
    div byte r0, byte r1
    invoke itoa, byte r0, itoaBuffer
    invoke printString, itoaBuffer
    invoke printChar, 10
    jmp @b

strDiv: db ' / ', 0
strEq: db ' = ', 0
itoaBuffer: db '-1234567890', 0

exceptionHandler:
    mov bp, sp

    invoke printChar, 10
.invalidOpcode:
    cmp r0, 0
    jne .divByZero
    invoke printString, msgUnknownOpcode
    jmp @f
.divByZero:
    cmp r0, 1
    jne .memoryBounds
    invoke printString, msgDivByZero
    jmp @f
.memoryBounds:
    cmp r0, 2
    jne .default
    invoke printString, msgMemoryBounds
    jmp @f
.default:
    invoke printString, msgUnknownException
@@:
    invoke printChar, ':'
    invoke printChar, 10

    mov r1, [bp + (0xC * 4)]
    mov r2, 5
@@:
    invoke disassemble, r1, disasmBuffer
    cmp r0, r0
    jz .error
    invoke printString, disasmBuffer
    invoke printChar, 10
    add r1, r0
    dec r2
    jnz @b
    jmp .return
.error:
    invoke printString, msgDisasmError

.return:
    jmp $ ; hang, returning will not help


; hardcoded interrupts best
interruptTable:
    dd exceptionHandler
    rd 31


disasmBuffer: rb 75

msgUnknownOpcode: db 'Unknown opcode', 0
msgDivByZero: db 'Divide by zero', 0
msgMemoryBounds: db 'Read/write out of memory bounds', 0
msgUnknownException: db 'Unknown exception', 0
msgDisasmError: db 'Disassemble failed', 0

include 'lib/string.asm'
include 'lib/term.asm'
include 'lib/disasm.asm'
