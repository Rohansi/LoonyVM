include 'loonyvm.inc'

; setup interrupts
ivt interruptTable
sti

mov r0, tasks
mov [r0 + TASK.State], 1
mov [r0 + TASK.Regs.IP], task1
mov [r0 + TASK.Regs.SP], 0x70000

add r0, sizeof.TASK
mov [r0 + TASK.State], 1
mov [r0 + TASK.Regs.IP], task2
mov [r0 + TASK.Regs.SP], 0x77FFF

; set timer interrupt to 100hz
mov r0, 1
mov r1, 100
int 1

; enable timer
mov r0, 0
mov r1, 1
int 1

; enable keyboard
mov r0, 0
mov r1, 1
int 2

; switch to right stack
mov sp, 0x70000

task1:
    invoke puts, msgPrompt
    invoke gets, nameBuff, 32
    invoke_va printf, msgResponseFmt, nameBuff
    jmp task1

nameBuff: rb 32
msgPrompt: db 'what is your name? ', 0
msgResponseFmt: db 'hello %s!!', 10, 10, 0

task2:
    xor r3, r3
.draw:
    mov r1, termSizeX * termSizeY
    mov r2, 1
    inc r3
    rem r3, 255
@@:
    cmp r1, r1
    jz .draw

    push r2
    dec r2
    div r2, 2
    mov r4, r2
    rem r4, termSizeX ; x = i % width
    mov r5, r2
    div r5, termSizeX ; y = i / width
    pop r2

    add r4, r3
    add r5, r3
    xor r4, r5
    and r4, 01110000b
    or  r4, 00001111b

    mov byte [r2 + termAddr], byte r4
    dec r1
    add r2, 2
    jmp @b

timerInterruptHandler:
    mov bp, sp

    ; save current task
    mov r0, [currTask]
    mul r0, sizeof.TASK
    add r0, tasks
    mov [r0 + TASK.Regs.R0],    [bp + REGISTERS.R0]
    mov [r0 + TASK.Regs.R1],    [bp + REGISTERS.R1]
    mov [r0 + TASK.Regs.R2],    [bp + REGISTERS.R2]
    mov [r0 + TASK.Regs.R3],    [bp + REGISTERS.R3]
    mov [r0 + TASK.Regs.R4],    [bp + REGISTERS.R4]
    mov [r0 + TASK.Regs.R5],    [bp + REGISTERS.R5]
    mov [r0 + TASK.Regs.R6],    [bp + REGISTERS.R6]
    mov [r0 + TASK.Regs.R7],    [bp + REGISTERS.R7]
    mov [r0 + TASK.Regs.R8],    [bp + REGISTERS.R8]
    mov [r0 + TASK.Regs.R9],    [bp + REGISTERS.R9]
    mov [r0 + TASK.Regs.BP],    [bp + REGISTERS.BP]
    mov [r0 + TASK.Regs.IP],    [bp + REGISTERS.IP]
    mov [r0 + TASK.Regs.SP],    [bp + REGISTERS.SP]
    mov [r0 + TASK.Regs.Flags], [bp + REGISTERS.Flags]

    ; find next task
    mov r1, [currTask]
.search:
    inc r1
    add r0, sizeof.TASK
    rem r1, MAX_TASKS
    jnz @f
    mov r0, tasks
@@:
    cmp byte [r0 + TASK.State], 0
    je .search

    ; restore new task
    mov [bp + REGISTERS.R0],    [r0 + TASK.Regs.R0]
    mov [bp + REGISTERS.R1],    [r0 + TASK.Regs.R1]
    mov [bp + REGISTERS.R2],    [r0 + TASK.Regs.R2]
    mov [bp + REGISTERS.R3],    [r0 + TASK.Regs.R3]
    mov [bp + REGISTERS.R4],    [r0 + TASK.Regs.R4]
    mov [bp + REGISTERS.R5],    [r0 + TASK.Regs.R5]
    mov [bp + REGISTERS.R6],    [r0 + TASK.Regs.R6]
    mov [bp + REGISTERS.R7],    [r0 + TASK.Regs.R7]
    mov [bp + REGISTERS.R8],    [r0 + TASK.Regs.R8]
    mov [bp + REGISTERS.R9],    [r0 + TASK.Regs.R9]
    mov [bp + REGISTERS.BP],    [r0 + TASK.Regs.BP]
    mov [bp + REGISTERS.IP],    [r0 + TASK.Regs.IP]
    mov [bp + REGISTERS.SP],    [r0 + TASK.Regs.SP]
    mov [bp + REGISTERS.Flags], [r0 + TASK.Regs.Flags]
    mov [currTask], r1

    iret

interruptTable:
    dd 0 ; cpu
    dd timerInterruptHandler
    dd kbInterruptHandler
    rd 29

currTask: dd 0
tasks: db sizeof.TASK * MAX_TASKS dup 0

MAX_TASKS = 4

struct REGISTERS
    R0      dd ?
    R1      dd ?
    R2      dd ?
    R3      dd ?
    R4      dd ?
    R5      dd ?
    R6      dd ?
    R7      dd ?
    R8      dd ?
    R9      dd ?
    BP      dd ?
    IP      dd ?
    SP      dd ?
    Flags   dd ?
ends

struct TASK
    State   db ?
    Regs    REGISTERS
ends

include 'lib/string.asm'
include 'lib/printf.asm'
include 'lib/term.asm'
include 'lib/keyboard.asm'
