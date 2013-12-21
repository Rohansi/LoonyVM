include 'loonyvm.inc'

@@:
	rand r0
	invoke itoa, r0, itoaBuffer1
	invoke strlen, itoaBuffer1
	invoke puts, str1
	invoke puts, itoaBuffer1
	invoke puts, str2
	invoke strlen, itoaBuffer1
	invoke itoa, r0, itoaBuffer2
	invoke puts, itoaBuffer2
	invoke putc, 32 ; space
	jmp @b

str1: db 'strlen("', 0
str2: db '") = ', 0
itoaBuffer1: db '-1234567890', 0
itoaBuffer2: db '-1234567890', 0

include 'string.inc'
include 'term.inc'
