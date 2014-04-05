include 'loonyvm.inc'

mov r0, 10

.while r0 >= 0
	.if r0 = 5
		invoke printString, msgFive
	.else
		invoke_va printf, msgCountFmt, r0
	.endif

	dec r0
.endw

jmp $

msgCountFmt: db 'n = %i', 10, 0
msgFive: db 'hi im 5', 10, 0

include 'lib/string.asm'
include 'lib/printf.asm'
include 'lib/term.asm'
