
namespace LoonyVM
{
    internal static class Util
    {
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
    }
}
