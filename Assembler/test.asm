include 'loonyvm.inc'

inc r1
int 6
jmp $

rb 0x60000 - ($-$$)
file 'out.pic'
