; http://board.flatassembler.net/topic.php?p=142613#142613

format binary as 'bin'

include 'loonyproc.inc'
include 'struct.inc'
include 'if.inc'

r0 equ eax
r1 equ rax
r2 equ ebx
r3 equ rbx
r4 equ ecx
r5 equ rcx
r6 equ edx
r7 equ rdx
r8 equ ebp
r9 equ rbp
bp equ esi
sp equ eip
ip equ rip

@r0 = 0x00
@r1 = 0x01
@r2 = 0x02
@r3 = 0x03
@r4 = 0x04
@r5 = 0x05
@r6 = 0x06
@r7 = 0x07
@r8 = 0x08
@r9 = 0x09
@bp = 0x0A
@ip = 0x0B
@sp = 0x0C

@immb = 0x0D
@immw = 0x0E
@immd = 0x0F

@mov     = 0x00
@add     = 0x01
@sub     = 0x02
@mul     = 0x03
@div     = 0x04
@rem     = 0x05
@inc     = 0x06
@dec     = 0x07
@not     = 0x08
@and     = 0x09
@or      = 0x0A
@xor     = 0x0B
@shl     = 0x0C
@shr     = 0x0D
@push    = 0x0E
@pop     = 0x0F
@jmp     = 0x10
@call    = 0x11
@ret     = 0x12
@cmp     = 0x13
@jz      = 0x14
@jnz     = 0x15
@je      = 0x16
@jne     = 0x17
@ja      = 0x18
@jae     = 0x19
@jb      = 0x1A
@jbe     = 0x1B
@rand    = 0x1C
@int     = 0x1D
@iret    = 0x1E
@ivt     = 0x1F
@abs     = 0x20
@retn    = 0x21 
@xchg    = 0x22
@cmpxchg = 0x23
@pusha   = 0x24
@popa    = 0x25
@sti     = 0x26
@cli     = 0x27

macro _pParseOperand value {
    define v value
    
    @operand = 0
    @operandType = 2
    @isPtr = 0
    @isOffset = 0
    @isNegOffset = 0
    @offsetReg = 0
    @hasPayload = 0
    @payloadType = 2
    @payload = 0

    match =byte op,v \{
        define v op
        @operandType = 0
    \}
    
    match =word op,v \{
        define v op
        @operandType = 1
    \}
    
    match =dword op,v \{
        define v op
        @operandType = 2
    \}
    
    match [addr],v \{
        define v addr
        @isPtr = 1
    \}
    
    match base + expr,v \{
        define v expr
        if base eqtype eax
    irps reg, r0 r1 r2 r3 r4 r5 r6 r7 r8 r9 bp ip sp \\{
            if base eq reg
                @isOffset = 1
                @isNegOffset = 0
                @offsetReg = @\\#reg
            end if
    \\}
        else
            display 'error: expression must be relative to a register', 10
            assert 0
        end if
    \}

    match base - expr,v \{
        define v expr
        if base eqtype eax
    irps reg, r0 r1 r2 r3 r4 r5 r6 r7 r8 r9 bp ip sp \\{
            if base eq reg
                @isOffset = 1
                @isNegOffset = 1
                @offsetReg = @\\#reg
            end if
    \\}
        else
            display 'error: expression must be relative to a register', 10
            assert 0
        end if
    \}
    
    match expr,v \{
        define v
        
        if 0 = 1
    irps reg, r0 r1 r2 r3 r4 r5 r6 r7 r8 r9 bp ip sp \\{
        else if expr eq reg
            if @isNegOffset
                display 'error: can not have negative register offset', 10
                assert 0
            end if

            @operand = @\\#reg
    \\}
        else
            @hasPayload = 1
            @a = dword expr

            if @isNegOffset
                @a = 0 - @a
            end if
            
            if @a <= 127
                @operand = @immb
                @payloadType = 0
                @payload = byte expr
            else if @a <= 32767
                @operand = @immw
                @payloadType = 1
                @payload = word expr
            else if @a <= 2147483647
                @operand = @immd
                @payloadType = 2
                @payload = dword expr
            else
                @operand = @immd     ; TODO: make negatives space efficient
                @payloadType = 2
                @payload = dword expr
            end if
        end if
    \}
}

macro _pInstruction2 opcode,a,b {
    @b0 = opcode
    @b1 = 0
    @b2 = 0
    @b3 = 0
    @hasP1 = 0
    @p1Type = 2
    @p1 = 0
    @hasP2 = 0
    @p1Type = 2
    @p2 = 0
    
    _pParseOperand a
    @b1 = @b1 or (@operand shl 4)
    @b3 = @b3 or (@operandType shl 4)
    if @isPtr = 1
        @b3 = @b3 or 0x80
    end if
    if @isOffset = 1
        @b3 = @b3 or 0x40
        @b2 = @b2 or (@offsetReg shl 4)
    end if
    if @hasPayload = 1
        @hasP1 = 1
        @p1Type = @payloadType
        @p1 = @payload
    end if
    
    _pParseOperand b
    @b1 = @b1 or @operand
    @b3 = @b3 or @operandType
    if @isPtr > 0
        @b3 = @b3 or 0x08
    end if
    if @isOffset = 1
        @b3 = @b3 or 0x04
        @b2 = @b2 or (@offsetReg and 15)
    end if
    if @hasPayload = 1
        @hasP2 = 1
        @p2Type = @payloadType
        @p2 = @payload
    end if
    
    db @b0, @b1, @b2, @b3
    
    if @hasP1 = 1
        if @p1Type = 0
            db @p1
        else if @p1Type = 1
            dw @p1
        else if @p1Type = 2
            dd @p1
        else
            assert 0
        end if
    end if
    if @hasP2 = 1
        if @p2Type = 0
            db @p2
        else if @p2Type = 1
            dw @p2
        else if @p2Type = 2
            dd @p2
        else
            assert 0
        end if
    end if
}

macro _pInstruction1 opcode,a {
    @b0 = opcode
    @b1 = 0
    @b2 = 0
    @b3 = 0
    @hasP1 = 0
    @p1Type = 2
    @p1 = 0
    
    _pParseOperand a
    @b1 = @b1 or (@operand shl 4)
    @b3 = @b3 or (@operandType shl 4)
    if @isPtr = 1
        @b3 = @b3 or 0x80
    end if
    if @isOffset = 1
        @b3 = @b3 or 0x40
        @b2 = @b2 or (@offsetReg shl 4)
    end if
    if @hasPayload = 1
        @hasP1 = 1
        @p1Type = @payloadType
        @p1 = @payload
    end if
    
    db @b0, @b1, @b2, @b3
    
    if @hasP1 = 1
        if @p1Type = 0
            db @p1
        else if @p1Type = 1
            dw @p1
        else if @p1Type = 2
            dd @p1
        else
            assert 0
        end if
    end if
}

macro _pInstruction0 opcode {
    @b0 = opcode
    @b1 = 0
    @b2 = 0
    @b3 = 0
    
    db @b0, @b1, @b2, @b3
}

irps instr, mov add sub mul div rem and or xor shl shr cmp xchg cmpxchg { 
    macro instr a,b \{
        _pInstruction2 @\#instr,a,b
    \} 
}

irps instr, inc dec not push pop jmp call jz jnz je jne ja jae jb jbe rand int ivt abs retn { 
    macro instr a \{
        _pInstruction1 @\#instr,a
    \} 
}

irps instr, ret iret pusha popa sti cli { 
    macro instr \{
        _pInstruction0 @\#instr
    \} 
}

