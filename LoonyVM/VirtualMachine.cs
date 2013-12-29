using System;

namespace LoonyVM
{
    public partial class VirtualMachine
    {
        [Flags]
        private enum Flags
        {
            Zero = 1 << 0,
            Equal = 1 << 1,
            Above = 1 << 2,
            Below = 1 << 3,

            None = 0
        }

        private static readonly Random Random = new Random();

        public readonly int[] Registers;

        public int IP
        {
            get { return Registers[0xB]; }
            set { Registers[0xB] = value; }
        }

        public int SP
        {
            get { return Registers[0xC]; }
            set { Registers[0xC] = value; }
        }

        public readonly byte[] Memory;

        private Flags _flags;
        private Instruction _instruction;
        private bool _interruptsEnabled;
        private bool _interrupted;
        private int _ivt;
        private IDevice[] _devices;
        private int _errorIp;

        public VirtualMachine(int memorySize)
        {
            if (memorySize < 512 * 1024)
                throw new ArgumentOutOfRangeException("memorySize", "VM should have at least 512kB of memory");

            Registers = new int[13];
            Memory = new byte[memorySize];

            IP = 0;
            SP = Memory.Length;

            _flags = Flags.None;
            _instruction = new Instruction(this);
            _interruptsEnabled = false;
            _interrupted = false;
            _ivt = 0;
            _devices = new IDevice[16];

            Attach(this);
        }

        public void Attach(IDevice device)
        {
            if (_devices[device.Id] != null)
                throw new Exception("Duplicate device id");

            _devices[device.Id] = device;
        }

        public void Step()
        {
            _errorIp = IP;

            try
            {
                if (_interruptsEnabled && !_interrupted)
                {
                    for (var i = 0; i < _devices.Length; i++)
                    {
                        var device = _devices[i];
                        if (device == null || !device.InterruptRequest)
                            continue;

                        Interrupt(device.Id);
                        device.HandleInterruptRequest(this);
                        break;
                    }
                }

                _instruction.Decode();
                //Console.WriteLine(_instruction);

                int result;
                switch (_instruction.Opcode)
                {
                    case Opcode.Mov:
                        _instruction.Left.Set(_instruction.Right.Get());
                        break;
                    case Opcode.Add:
                        result = _instruction.Left.Get() + _instruction.Right.Get();
                        _instruction.Left.Set(result);
                        SetZero(result);
                        break;
                    case Opcode.Sub:
                        result = _instruction.Left.Get() - _instruction.Right.Get();
                        _instruction.Left.Set(result);
                        SetZero(result);
                        break;
                    case Opcode.Mul:
                        result = _instruction.Left.Get() * _instruction.Right.Get();
                        _instruction.Left.Set(result);
                        SetZero(result);
                        break;
                    case Opcode.Div:
                        result = _instruction.Left.Get() / _instruction.Right.Get();
                        _instruction.Left.Set(result);
                        SetZero(result);
                        break;
                    case Opcode.Rem:
                        result = _instruction.Left.Get() % _instruction.Right.Get();
                        _instruction.Left.Set(result);
                        SetZero(result);
                        break;
                    case Opcode.Inc:
                        result = _instruction.Left.Get() + 1;
                        _instruction.Left.Set(result);
                        SetZero(result);
                        break;
                    case Opcode.Dec:
                        result = _instruction.Left.Get() - 1;
                        _instruction.Left.Set(result);
                        SetZero(result);
                        break;
                    case Opcode.Not:
                        result = ~_instruction.Left.Get();
                        _instruction.Left.Set(result);
                        SetZero(result);
                        break;
                    case Opcode.And:
                        result = _instruction.Left.Get() & _instruction.Right.Get();
                        _instruction.Left.Set(result);
                        SetZero(result);
                        break;
                    case Opcode.Or:
                        result = _instruction.Left.Get() | _instruction.Right.Get();
                        _instruction.Left.Set(result);
                        SetZero(result);
                        break;
                    case Opcode.Xor:
                        result = _instruction.Left.Get() ^ _instruction.Right.Get();
                        _instruction.Left.Set(result);
                        SetZero(result);
                        break;
                    case Opcode.Shl:
                        result = _instruction.Left.Get() << _instruction.Right.Get();
                        _instruction.Left.Set(result);
                        SetZero(result);
                        break;
                    case Opcode.Shr:
                        result = _instruction.Left.Get() >> _instruction.Right.Get();
                        _instruction.Left.Set(result);
                        SetZero(result);
                        break;
                    case Opcode.Push:
                        Push(_instruction.Left.Get());
                        break;
                    case Opcode.Pop:
                        _instruction.Left.Set(Pop());
                        break;
                    case Opcode.Jmp:
                        IP = _instruction.Left.Get();
                        break;
                    case Opcode.Call:
                        Push(IP);
                        IP = _instruction.Left.Get();
                        break;
                    case Opcode.Ret:
                        IP = Pop();
                        break;
                    case Opcode.Cmp:
                        var cmpValL = _instruction.Left.Get();
                        var cmpValR = _instruction.Right.Get();

                        _flags = Flags.None;
                        if (cmpValL == 0)
                            _flags |= Flags.Zero;
                        if (cmpValL == cmpValR)
                            _flags |= Flags.Equal;
                        if (cmpValL > cmpValR)
                            _flags |= Flags.Above;
                        if (cmpValL < cmpValR)
                            _flags |= Flags.Below;
                        break;
                    case Opcode.Jz:
                        if ((_flags & Flags.Zero) != 0)
                            IP = _instruction.Left.Get();
                        break;
                    case Opcode.Jnz:
                        if ((_flags & Flags.Zero) == 0)
                            IP = _instruction.Left.Get();
                        break;
                    case Opcode.Je:
                        if ((_flags & Flags.Equal) != 0)
                            IP = _instruction.Left.Get();
                        break;
                    case Opcode.Jne:
                        if ((_flags & Flags.Equal) == 0)
                            IP = _instruction.Left.Get();
                        break;
                    case Opcode.Ja:
                        if ((_flags & Flags.Above) != 0)
                            IP = _instruction.Left.Get();
                        break;
                    case Opcode.Jae:
                        if ((_flags & Flags.Above) != 0 || (_flags & Flags.Equal) != 0)
                            IP = _instruction.Left.Get();
                        break;
                    case Opcode.Jb:
                        if ((_flags & Flags.Below) != 0)
                            IP = _instruction.Left.Get();
                        break;
                    case Opcode.Jbe:
                        if ((_flags & Flags.Below) != 0 || (_flags & Flags.Equal) != 0)
                            IP = _instruction.Left.Get();
                        break;
                    case Opcode.Rand:
                        _instruction.Left.Set(Random.Next(int.MinValue, int.MaxValue));
                        break;
                    case Opcode.Int:
                        var intVal = _instruction.Left.Get();
                        if (intVal < 0 || intVal >= _devices.Length || _devices[intVal] == null)
                            break;
                        _devices[intVal].HandleInterrupt(this);
                        break;
                    case Opcode.Iret:
                        InterruptReturn();
                        break;
                    case Opcode.Ivt:
                        _ivt = _instruction.Left.Get();
                        break;
                    case Opcode.Abs:
                        _instruction.Left.Set(Math.Abs(_instruction.Left.Get()));
                        break;
                    case Opcode.Retn:
                        IP = Pop();
                        SP += _instruction.Left.Get();
                        break;
                    case Opcode.Xchg:
                        var xt = _instruction.Left.Get();
                        _instruction.Left.Set(_instruction.Right.Get());
                        _instruction.Right.Set(xt);
                        break;
                    case Opcode.Cmpxchg:
                        if (Registers[0] == _instruction.Left.Get())
                        {
                            _flags |= Flags.Equal;
                            _instruction.Left.Set(_instruction.Right.Get());
                        }
                        else
                        {
                            _flags &= ~Flags.Equal;
                            Registers[0] = _instruction.Left.Get();
                        }
                        break;
                    case Opcode.Pusha:
                        for (var i = 9; i >= 0; i--)
                        {
                            Push(Registers[i]);
                        }
                        break;
                    case Opcode.Popa:
                        for (var i = 0; i <= 9; i++)
                        {
                            Registers[i] = Pop();
                        }
                        break;
                    case Opcode.Sti:
                        _interruptsEnabled = true;
                        break;
                    case Opcode.Cli:
                        _interruptsEnabled = false;
                        break;
                    default:
                        throw new VirtualMachineInvalidOpcode("Bad opcode id");
                }
            }
            catch (VirtualMachineInvalidOpcode)
            {
                Exception(ExceptionCode.InvalidOpcode);
            }
            catch (IndexOutOfRangeException)
            {
                Exception(ExceptionCode.MemoryBounds);
            }
            catch (DivideByZeroException)
            {
                Exception(ExceptionCode.DivideByZero);
            }
            catch (VirtualMachineException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new VirtualMachineException(_errorIp, "Error: " + e, e);
            }
        }

        public void Interrupt(byte index)
        {
            if (!_interruptsEnabled)
                throw new VirtualMachineException(_errorIp, "Can't interrupt when they are disabled");

            if (_interrupted)
                throw new VirtualMachineException(_errorIp, "Can't interrupt while interrupted");

            var sp = SP;
            Push((int)_flags);
            Push(sp);
            Push(IP);

            for (var i = 10; i >= 0; i--)
            {
                Push(Registers[i]);
            }

            IP = Memory.ReadInt(_ivt + (index * sizeof(int)));

            _interrupted = true;
        }

        private void InterruptReturn()
        {
            for (var i = 0; i <= 10; i++)
            {
                Registers[i] = Pop();
            }

            IP = Pop();
            var sp = Pop();
            _flags = (Flags)Pop();
            SP = sp;

            _interrupted = false;
        }

        private void Push(int value)
        {
            SP -= sizeof(int);
            Memory.WriteInt(SP, value);
        }

        private int Pop()
        {
            var value = Memory.ReadInt(SP);
            SP += sizeof(int);
            return value;
        }

        private void SetZero(int value)
        {
            _flags &= ~Flags.Zero;
            if (value == 0)
                _flags |= Flags.Zero;
        }
    }
}
