include 'loonyvm.inc'

@@:
    rand r0
    invoke itoa, r0, itoaBuffer1
    invoke strlen, itoaBuffer1
    invoke printString, str1
    invoke printString, itoaBuffer1
    invoke printString, str2
    invoke strlen, itoaBuffer1
    invoke itoa, r0, itoaBuffer2
    invoke printString, itoaBuffer2
    invoke printChar, 32 ; space
    jmp @b

str1: db 'strlen("', 0
str2: db '") = ', 0
itoaBuffer1: db '-1234567890', 0
itoaBuffer2: db '-1234567890', 0

include 'lib/string.asm'
include 'lib/term.asm'
