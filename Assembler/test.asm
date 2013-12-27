include 'loonyvm.inc'

; setup interrupts
ivt interruptTable
sti

; enable keyboard
mov r0, 0
mov r1, 1
int 2

main:
    invoke kbIsPressed, ' '
    cmp r0, r0
    jz @f
    invoke puts, msgPressed

@@:
    invoke kbDequeue

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

    invoke putc, r0

    jmp main

msgPressed: db 'BOOP', 0

interruptTable:
    dd 0 ; cpu
    dd 0 ; timer
    dd kbInterruptHandler
    rd 29

include 'lib/term.asm'
include 'lib/keyboard.asm'
