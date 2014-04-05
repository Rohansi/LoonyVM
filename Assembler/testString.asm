include 'loonyvm.inc'

@@:
    rand byte r1
    rand byte r2
    invoke printString, str1
    invoke itoa, byte r1, itoaBuffer1
    invoke printString, itoaBuffer1
    invoke printString, str2
    invoke itoa, byte r2, itoaBuffer2
    invoke printString, itoaBuffer2
    invoke printString, str3
    invoke strcmp, itoaBuffer1, itoaBuffer2
    invoke itoa, r0, itoaBuffer1
    invoke printString, itoaBuffer1
    invoke printChar, 10
    jmp @b

str1: db 'strcmp("', 0
str2: db '", "', 0
str3: db '") = ', 0
itoaBuffer1: db '-1234567890', 0
itoaBuffer2: db '-1234567890', 0

include 'lib/string.asm'
include 'lib/term.asm'
