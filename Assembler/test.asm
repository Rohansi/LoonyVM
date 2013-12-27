include 'loonyvm.inc'

; setup interrupts
ivt interruptTable
sti

; enable keyboard
mov r0, 0
mov r1, 1
int 2

main:
    invoke kbRead

    ; make sure we get an event
    cmp r0, -1
    je main

    ; filter out key releases
    mov r1, r0
    and r1, 0xFF00
    jz main

    ; only care about printable characters
    and r0, 0x00FF
    cmp r0, 32
    jb main
    cmp r0, 127
    ja main

    int 0
    invoke putc, r0

    jmp main


keyboardHandler:
    mov r2, r0
    shl r2, 8
    or r2, r1
    invoke kbWrite, word r2
.return:
    iret

interruptTable:
    dd 0 ; cpu
    dd 0 ; timer
    dd keyboardHandler
    rd 29

include 'lib/string.asm'
include 'lib/term.asm'
include 'lib/keyboard.asm'
