
namespace LoonyVM
{
    internal static class Util
    {
        #region Memory I/O - VM
        public static sbyte ReadSByte(this VirtualMachine machine, int offset)
        {
            return machine.Memory.ReadSByte(machine.Origin + offset);
        }

        public static short ReadShort(this VirtualMachine machine, int offset)
        {
            return machine.Memory.ReadShort(machine.Origin + offset);
        }

        public static int ReadInt(this VirtualMachine machine, int offset)
        {
            return machine.Memory.ReadInt(machine.Origin + offset);
        }

        public static void WriteSByte(this VirtualMachine machine, int offset, sbyte value)
        {
            machine.Memory.WriteSByte(machine.Origin + offset, value);
        }

        public static void WriteShort(this VirtualMachine machine, int offset, short value)
        {
            machine.Memory.WriteShort(machine.Origin + offset, value);
        }

        public static void WriteInt(this VirtualMachine machine, int offset, int value)
        {
            machine.Memory.WriteInt(machine.Origin + offset, value);
        }
        #endregion

        #region Memory I/O - General
        public static sbyte ReadSByte(this byte[] buffer, int offset)
        {
            return (sbyte)buffer[offset];
        }

        public static short ReadShort(this byte[] buffer, int offset)
        {
            return (short)(buffer[offset + 0] | buffer[offset + 1] << 8);
        }

        public static int ReadInt(this byte[] buffer, int offset)
        {
            return buffer[offset + 0] |
                   buffer[offset + 1] << 8 |
                   buffer[offset + 2] << 16 |
                   buffer[offset + 3] << 24;
        }

        public static void WriteSByte(this byte[] buffer, int offset, sbyte value)
        {
            buffer[offset] = (byte)value;
        }

        public static void WriteShort(this byte[] buffer, int offset, short value)
        {
            buffer[offset + 0] = (byte)((value >> 0) & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        public static void WriteInt(this byte[] buffer, int offset, int value)
        {
            buffer[offset + 0] = (byte)((value >> 0) & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }
        #endregion

        public static float Clamp(int value, int min, int max)
        {
            return (value < min) ? min : ((value > max) ? max : value);
        }
    }
}
