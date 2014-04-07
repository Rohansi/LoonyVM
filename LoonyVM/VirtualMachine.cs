using System;

namespace LoonyVM
{
    public enum Opcode
    {
        Mov, Add, Sub, Mul, Div, Rem, Inc, Dec, Not, And, Or,
        Xor, Shl, Shr, Push, Pop, Jmp, Call, Ret, Cmp, Jz, Jnz,
        Je, Jne, Ja, Jae, Jb, Jbe, Rand, Int, Iret, Ivt, Abs,
        Retn, Xchg, Cmpxchg, Pusha, Popa, Sti, Cli, Neg, Count
    }

    public enum Register
    {
        R0, R1, R2, R3, R4, R5, R6, R7, R8, R9, BP, SP, IP, Count
    }

    [Flags]
    public enum VmFlags
    {
        Zero = 1 << 0,
        Equal = 1 << 1,
        Above = 1 << 2,
        Below = 1 << 3,

        None = 0
    }

    public partial class VirtualMachine : IDisposable
    {
        private static readonly Random Random = new Random();

        public readonly int[] Registers;
        public VmFlags Flags;
        public int IVT;

        public int IP
        {
            get { return Registers[(int)Register.IP]; }
            set { Registers[(int)Register.IP] = value; }
        }

        public int SP
        {
            get { return Registers[(int)Register.SP]; }
            set { Registers[(int)Register.SP] = value; }
        }

        public readonly byte[] Memory;

        private Instruction _instruction;
        private bool _interruptsEnabled;
        private bool _interrupted;
        private IDevice[] _devices;
        private int _errorIp;

        private Devices.Timer _timer;

        public VirtualMachine(int memorySize)
        {
            if (memorySize < 512 * 1024)
                throw new ArgumentOutOfRangeException("memorySize", "VM should have at least 512kB of memory");

            Registers = new int[(int)Register.Count];
            Memory = new byte[memorySize];

            IP = 0;
            SP = Memory.Length;

            Flags = VmFlags.None;
            IVT = 0;

            _instruction = new Instruction(this);
            _interruptsEnabled = false;
            _interrupted = false;
            _devices = new IDevice[16];

            Attach(this);

            _timer = new Devices.Timer();
            Attach(_timer);

            Attach(new Devices.SysCall());
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

                        Flags = VmFlags.None;
                        if (cmpValL == 0)
                            Flags |= VmFlags.Zero;
                        if (cmpValL == cmpValR)
                            Flags |= VmFlags.Equal;
                        if (cmpValL > cmpValR)
                            Flags |= VmFlags.Above;
                        if (cmpValL < cmpValR)
                            Flags |= VmFlags.Below;
                        break;
                    case Opcode.Jz:
                        if ((Flags & VmFlags.Zero) != 0)
                            IP = _instruction.Left.Get();
                        break;
                    case Opcode.Jnz:
                        if ((Flags & VmFlags.Zero) == 0)
                            IP = _instruction.Left.Get();
                        break;
                    case Opcode.Je:
                        if ((Flags & VmFlags.Equal) != 0)
                            IP = _instruction.Left.Get();
                        break;
                    case Opcode.Jne:
                        if ((Flags & VmFlags.Equal) == 0)
                            IP = _instruction.Left.Get();
                        break;
                    case Opcode.Ja:
                        if ((Flags & VmFlags.Above) != 0)
                            IP = _instruction.Left.Get();
                        break;
                    case Opcode.Jae:
                        if ((Flags & VmFlags.Above) != 0 || (Flags & VmFlags.Equal) != 0)
                            IP = _instruction.Left.Get();
                        break;
                    case Opcode.Jb:
                        if ((Flags & VmFlags.Below) != 0)
                            IP = _instruction.Left.Get();
                        break;
                    case Opcode.Jbe:
                        if ((Flags & VmFlags.Below) != 0 || (Flags & VmFlags.Equal) != 0)
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
                        IVT = _instruction.Left.Get();
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
                        if (Registers[(int)Register.R0] == _instruction.Left.Get())
                        {
                            Flags |= VmFlags.Equal;
                            _instruction.Left.Set(_instruction.Right.Get());
                        }
                        else
                        {
                            Flags &= ~VmFlags.Equal;
                            Registers[(int)Register.R0] = _instruction.Left.Get();
                        }
                        break;
                    case Opcode.Pusha:
                        for (var i = (int)Register.R9; i >= (int)Register.R0; i--)
                        {
                            Push(Registers[i]);
                        }
                        break;
                    case Opcode.Popa:
                        for (var i = (int)Register.R0; i <= (int)Register.R9; i++)
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
                    case Opcode.Neg:
                        _instruction.Left.Set(0 - _instruction.Left.Get());
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
                throw new VirtualMachineException(_errorIp, "Error: " + e);
            }
        }

        public void Interrupt(byte index)
        {
            if (!_interruptsEnabled)
                throw new VirtualMachineException(_errorIp, "Can't interrupt when they are disabled");

            if (_interrupted)
                throw new VirtualMachineException(_errorIp, "Can't interrupt while interrupted");

            Push(SP);
            Push(IP);
            Push((int)Flags);

            for (var i = (int)Register.BP; i >= (int)Register.R0; i--)
            {
                Push(Registers[i]);
            }

            IP = Memory.ReadInt(IVT + (index * sizeof(int)));

            _interrupted = true;
        }

        private void InterruptReturn()
        {
            for (var i = (int)Register.R0; i <= (int)Register.BP; i++)
            {
                Registers[i] = Pop();
            }

            Flags = (VmFlags)Pop();
            IP = Pop();
            SP = Pop();

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
            Flags &= ~VmFlags.Zero;
            if (value == 0)
                Flags |= VmFlags.Zero;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
