﻿using System;

namespace LoonyVM
{
    public class VirtualMachine
    {
        [Flags]
        private enum Flags : byte
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
        private bool _interrupted;
        private int _ivt;
        private Device[] _devices;
        private int _errorIp;

        public VirtualMachine()
        {
            Registers = new int[13];
            Memory = new byte[512 * 1024];

            IP = 0;
            SP = Memory.Length;

            _flags = Flags.None;
            _instruction = new Instruction(this);
            _interrupted = false;
            _ivt = 0;
            _devices = new Device[32];
        }

        public void Attach(Device device)
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
                if (_ivt != 0 && !_interrupted)
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
                        result = Math.Abs(_instruction.Left.Get());
                        _instruction.Left.Set(result);
                        SetZero(result);
                        break;
                    default:
                        throw new VirtualMachineException(_errorIp, "Invalid opcode");
                }
            }
            catch (IndexOutOfRangeException e)
            {
                throw new VirtualMachineException(_errorIp, "Out of memory bounds", e);
            }
            catch (DivideByZeroException e)
            {
                throw new VirtualMachineException(_errorIp, "Divide by zero", e);
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
            Push(SP);
            Push(IP);
            Push((int)_flags);

            for (var i = Registers.Length - 1; i >= 0; i--)
            {
                Push(Registers[i]);
            }

            IP = Memory.ReadInt(_ivt + (index * 4));

            _interrupted = true;
        }

        private void InterruptReturn()
        {
            for (var i = 0; i < Registers.Length; i++)
            {
                Registers[i] = Pop();
            }

            _flags = (Flags)Pop();
            IP = Pop();
            SP = Pop();

            _interrupted = false;
        }

        private void Push(int value)
        {
            SP -= 4;
            Memory.WriteInt(SP, value);
        }

        private int Pop()
        {
            var value = Memory.ReadInt(SP);
            SP += 4;
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
