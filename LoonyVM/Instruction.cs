using System.Collections.Generic;
using System.Text;

namespace LoonyVM
{
    public enum Opcode : byte
    {
        Mov, Add, Sub, Mul, Div, Rem, Inc, Dec, Not, And, Or,
        Xor, Shl, Shr, Push, Pop, Jmp, Call, Ret, Cmp, Jz, Jnz,
        Je, Jne, Ja, Jae, Jb, Jbe, Rand, Int, Iret, Ivt, Abs, 
        Retn, None
    }

    public class Instruction
    {
        public Opcode Opcode;
        public Operand Left;
        public Operand Right;

        private VirtualMachine _machine;

        public Instruction(VirtualMachine machine)
        {
            _machine = machine;

            Left = new Operand(machine);
            Right = new Operand(machine);
        }

        public void Decode()
        {
            Opcode = (Opcode)_machine.Memory[_machine.IP++];

            var b1 = _machine.Memory[_machine.IP++];
            var b2 = _machine.Memory[_machine.IP++];
            var b3 = _machine.Memory[_machine.IP++];

            var left = b1 >> 4;
            var leftPtr = (b3 & 0x80) != 0;
            var leftOffset = (b3 & 0x40) != 0;
            var leftOffsetReg = b2 >> 4;
            var leftType = (b3 >> 4) & 0x03;
            var leftPayload = ReadPayload(left);

            var right = b1 & 0x0F;
            var rightPtr = (b3 & 0x08) != 0;
            var rightOffset = (b3 & 0x04) != 0;
            var rightOffsetReg = b2 & 0x0F;
            var rightType = b3 & 0x03;
            var rightPayload = ReadPayload(right);

            Left.Change(left, leftType, leftPtr, leftOffset, leftOffsetReg, leftPayload);
            Right.Change(right, rightType, rightPtr, rightOffset, rightOffsetReg, rightPayload);
        }

        private int ReadPayload(int operandType)
        {
            var payload = 0;

            switch (operandType)
            {
                case 0xD:
                    payload = _machine.Memory.ReadSByte(_machine.IP);
                    _machine.IP += sizeof(sbyte);
                    break;
                case 0xE:
                    payload = _machine.Memory.ReadShort(_machine.IP);
                    _machine.IP += sizeof(short);
                    break;
                case 0xF:
                    payload = _machine.Memory.ReadInt(_machine.IP);
                    _machine.IP += sizeof(int);
                    break;
            }

            return payload;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var operands = OperandCounts[Opcode];

            sb.Append(Opcode.ToString().ToLower());

            if (operands >= 1)
            {
                sb.Append(' ');
                sb.Append(Left);
            }

            if (operands >= 2)
            {
                sb.Append(", ");
                sb.Append(Right);
            }

            return sb.ToString();
        }

        public static readonly Dictionary<Opcode, int> OperandCounts = new Dictionary<Opcode, int>
        {
            { Opcode.Mov,     2 },
            { Opcode.Add,     2 },
            { Opcode.Sub,     2 },
            { Opcode.Mul,     2 },
            { Opcode.Div,     2 },
            { Opcode.Rem,     2 },
            { Opcode.Inc,     1 },
            { Opcode.Dec,     1 },
            { Opcode.Not,     1 },
            { Opcode.And,     2 },
            { Opcode.Or,      2 },
            { Opcode.Xor,     2 },
            { Opcode.Shl,     2 },
            { Opcode.Shr,     2 },
            { Opcode.Push,    1 },
            { Opcode.Pop,     1 },
            { Opcode.Jmp,     1 },
            { Opcode.Call,    1 },
            { Opcode.Ret,     0 },
            { Opcode.Cmp,     2 },
            { Opcode.Jz,      1 },
            { Opcode.Jnz,     1 },
            { Opcode.Je,      1 },
            { Opcode.Jne,     1 },
            { Opcode.Ja,      1 },
            { Opcode.Jae,     1 },
            { Opcode.Jb,      1 },
            { Opcode.Jbe,     1 },
            { Opcode.Rand,    1 },
            { Opcode.Int,     1 },
            { Opcode.Iret,    0 },
            { Opcode.Ivt,     1 },
            { Opcode.Abs,     1 },
            { Opcode.Retn,    1 }
        };
    }
}
