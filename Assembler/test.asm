include 'loonyvm.inc'

; setup interrupts
ivt interruptTable
sti

; set timer interrupt to 100hz
mov r0, 1
mov r1, 100
int 1

; enable timer
mov r0, 0
mov r1, 1
int 1

@@:
    cli
    invoke puts, msgCode
    sti
    jmp @b

msgCode: db 'normal code', 0

timerHandler:
    invoke puts, msgTimer
    iret

msgTimer: db 'TIMER INTERRUPT', 0


interruptTable:
    dd 0
    dd timerHandler
    rd 30

include 'lib/string.asm'
include 'lib/term.asm'

