include 'loonyvm.inc'

; setup interrupts
ivt interruptTable
sti

; enable keyboard
mov r0, 0
mov r1, 1
int 2

jmp $

keyboardHandler:
    cmp r0, r0
    jz .return
    cmp r1, 32
    jb .return
    cmp r1, 127
    ja .return

    invoke putc, r1

.return:
    iret

interruptTable:
    dd 0 ; cpu
    dd 0 ; timer
    dd keyboardHandler
    rd 29

include 'lib/string.asm'
include 'lib/term.asm'
