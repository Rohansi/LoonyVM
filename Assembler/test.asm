include 'loonyvm.inc'

; setup interrupts
ivt interruptTable
sti

; enable keyboard
mov r0, 0
mov r1, 1
int 2

main:
    invoke puts, msgPrompt
    invoke gets, nameBuff, 32
    invoke_va printf, msgResponseFmt, nameBuff
    jmp main

nameBuff: rb 32
msgPrompt: db 'who are u? ', 0
msgResponseFmt: db 'hi %s!', 10, 0

interruptTable:
    dd 0 ; cpu
    dd 0 ; timer
    dd kbInterruptHandler
    rd 29

include 'lib/string.asm'
include 'lib/printf.asm'
include 'lib/term.asm'
include 'lib/keyboard.asm'
