include 'loonyvm.inc'

invoke_va printf, fmt, brian, 12345678, 0xDEADBEEF
jmp $

fmt: db 'hello %s!', 10, \
        'heres a number: %i', 10, \
        'and a pointer: %p', 10, \
        'out of args: %s', 10, \
        'bad format: %z', 10, \
        'escape: %%', 0

brian: db 'brian', 0

include 'lib/string.asm'
include 'lib/term.asm'
include 'lib/printf.asm'
