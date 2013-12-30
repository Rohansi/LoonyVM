include 'loonyvm.inc'

mov r0, 3    ; read sectors
mov r1, buff ; destination
mov r2, 0    ; start sector
mov r3, 2    ; sector count
int 8        ; run it

int 6
jmp $

buff:
