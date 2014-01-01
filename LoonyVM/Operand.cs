using System;
using System.Text;

namespace LoonyVM
{
    public class Operand
    {
        private VirtualMachine _machine;
        private int _type;
        private int _valueType;
        private bool _pointer;
        private bool _offset;
        private int _offsetRegister;
        private int _payload;

        public Operand(VirtualMachine machine)
        {
            _machine = machine;
        }
    
        public void Change(int newType, int newValueType, bool newPointer, bool newOffset, int newOffsetRegister, int newPayload)
        {
            _type = newType;
            _valueType = newValueType;
            _pointer = newPointer;
            _offset = newOffset;
            _offsetRegister = newOffsetRegister;
            _payload = newPayload;
        }

        public int Get(bool resolvePointer = true, bool disableType = false)
        {
            int value = 0;
            if (_type <= 0xC)
                value = _machine.Registers[_type];
            if (_type >= 0xD && _type <= 0xF)
                value = _payload;

            if (_offset)
                value += _machine.Registers[_offsetRegister];

            if (_pointer && resolvePointer)
                value = _machine.ReadInt(value);

            if (disableType)
                return value;

            switch (_valueType)
            {
                case 0x0:
                    value = (sbyte)value;
                    break;
                case 0x1:
                    value = (short)value;
                    break;
                case 0x2:
                    break;
                default:
                    throw new VirtualMachineInvalidOpcode("Invalid operand value type");
            }

            return value;
        }

        public void Set(int value)
        {
            if (!_pointer)
            {
                if (_type <= 0xC)
                    _machine.Registers[_type] = PreserveUpper(value, _machine.Registers[_type], _valueType);
                return;
            }

            var val = PreserveUpper(value, Get(disableType: true), _valueType);
            var addr = Get(false, true);
            _machine.WriteInt(addr, val);
        }

        public static int PreserveUpper(int newValue, int originalValue, int type)
        {
            switch (type)
            {
                case 0x0:
                    return (int)(originalValue & 0xFFFFFF00) | ((sbyte)newValue) & 0xFF;
                case 0x1:
                    return (int)(originalValue & 0xFFFF0000) | ((short)newValue) & 0xFFFF;
                case 0x2:
                    return newValue;
                default:
                    throw new VirtualMachineInvalidOpcode("Invalid operand value type");
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            switch (_valueType)
            {
                case 0x0:
                    sb.Append("byte ");
                    break;
                case 0x1:
                    sb.Append("word ");
                    break;
                case 0x2:
                    break;
                default:
                    throw new Exception("Invalid operand value type");
            }

            if (_pointer)
                sb.Append('[');

            if (_offset)
            {
                if (_offsetRegister <= 0x9)
                    sb.AppendFormat("r{0:x}", _offsetRegister);
                if (_offsetRegister == 0xA)
                    sb.Append("bp");
                if (_offsetRegister == 0xB)
                    sb.Append("ip");
                if (_offsetRegister == 0xC)
                    sb.Append("sp");
                sb.Append(" + ");
            }

            if (_type <= 0x9)
                sb.AppendFormat("r{0:x}", _type);
            if (_type == 0xA)
                sb.Append("bp");
            if (_type == 0xB)
                sb.Append("ip");
            if (_type == 0xC)
                sb.Append("sp");
            if (_type >= 0xD && _type <= 0xF)
                sb.Append(_payload);

            if (_pointer)
                sb.Append(']');

            return sb.ToString();
        }
    }
}
