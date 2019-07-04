using System;

namespace Fregata.Buffers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/6/3 14:02:41
    /// </summary>
    public class BitHelper
    {
        public static short SwapInt16(short v)
        {
            return (short)(((v & 0xff) << 8) | ((v >> 8) & 0xff));
        }

        public static ushort SwapUInt16(ushort v)
        {
            return (ushort)(((v & 0xff) << 8) | ((v >> 8) & 0xff));
        }

        public static int SwapInt32(int v)
        {
            return (int)(((SwapInt16((short)v) & 0xffff) << 0x10) | (SwapInt16((short)(v >> 0x10)) & 0xffff));
        }

        public static uint SwapUInt32(uint v)
        {
            return (uint)(((SwapUInt16((ushort)v) & 0xffff) << 0x10) | (SwapUInt16((ushort)(v >> 0x10)) & 0xffff));
        }

        public static long SwapInt64(long v)
        {
            return (long)(((SwapInt32((int)v) & 0xffffffffL) << 0x20) | (SwapInt32((int)(v >> 0x20)) & 0xffffffffL));
        }

        public static ulong SwapUInt64(ulong v)
        {
            return (ulong)(((SwapUInt32((uint)v) & 0xffffffffL) << 0x20) | (SwapUInt32((uint)(v >> 0x20)) & 0xffffffffL));
        }

        public static void Write(Span<byte> _buffer, short value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
        }

        public static void Write(Span<byte> _buffer, ushort value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
        }

        public static void Write(Span<byte> _buffer, int value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
        }

        public static void Write(Span<byte> _buffer, uint value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
        }

        public static void Write(Span<byte> _buffer, long value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            _buffer[4] = (byte)(value >> 32);
            _buffer[5] = (byte)(value >> 40);
            _buffer[6] = (byte)(value >> 48);
            _buffer[7] = (byte)(value >> 56);
        }

        public static void Write(Span<byte> _buffer, ulong value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            _buffer[4] = (byte)(value >> 32);
            _buffer[5] = (byte)(value >> 40);
            _buffer[6] = (byte)(value >> 48);
            _buffer[7] = (byte)(value >> 56);
        }

        public static void Write(byte[] _buffer, int postion, short value)
        {
            _buffer[postion + 0] = (byte)value;
            _buffer[postion + 1] = (byte)(value >> 8);
        }

        public static void Write(byte[] _buffer, int postion, ushort value)
        {
            _buffer[postion + 0] = (byte)value;
            _buffer[postion + 1] = (byte)(value >> 8);
        }

        public static void Write(byte[] _buffer, int postion, int value)
        {
            _buffer[postion + 0] = (byte)value;
            _buffer[postion + 1] = (byte)(value >> 8);
            _buffer[postion + 2] = (byte)(value >> 16);
            _buffer[postion + 3] = (byte)(value >> 24);
        }

        public static void Write(byte[] _buffer, int postion, uint value)
        {
            _buffer[postion + 0] = (byte)value;
            _buffer[postion + 1] = (byte)(value >> 8);
            _buffer[postion + 2] = (byte)(value >> 16);
            _buffer[postion + 3] = (byte)(value >> 24);
        }

        public static void Write(byte[] _buffer, int postion, long value)
        {
            _buffer[postion + 0] = (byte)value;
            _buffer[postion + 1] = (byte)(value >> 8);
            _buffer[postion + 2] = (byte)(value >> 16);
            _buffer[postion + 3] = (byte)(value >> 24);
            _buffer[postion + 4] = (byte)(value >> 32);
            _buffer[postion + 5] = (byte)(value >> 40);
            _buffer[postion + 6] = (byte)(value >> 48);
            _buffer[postion + 7] = (byte)(value >> 56);
        }

        public static void Write(byte[] _buffer, int postion, ulong value)
        {
            _buffer[postion + 0] = (byte)value;
            _buffer[postion + 1] = (byte)(value >> 8);
            _buffer[postion + 2] = (byte)(value >> 16);
            _buffer[postion + 3] = (byte)(value >> 24);
            _buffer[postion + 4] = (byte)(value >> 32);
            _buffer[postion + 5] = (byte)(value >> 40);
            _buffer[postion + 6] = (byte)(value >> 48);
            _buffer[postion + 7] = (byte)(value >> 56);
        }

        public static short ReadInt16(byte[] m_buffer, int postion)
        {
            return (short)(m_buffer[postion + 0] | m_buffer[postion + 1] << 8);
        }

        public static ushort ReadUInt16(byte[] m_buffer, int postion)
        {
            return (ushort)(m_buffer[postion + 0] | m_buffer[postion + 1] << 8);
        }

        public static int ReadInt32(byte[] m_buffer, int postion)
        {
            return m_buffer[postion + 0] | m_buffer[postion + 1] << 8 | m_buffer[postion + 2] << 16 | m_buffer[postion + 3] << 24;
        }

        public static uint ReadUInt32(byte[] m_buffer, int postion)
        {
            return (uint)(m_buffer[postion + 0] | m_buffer[postion + 1] << 8 | m_buffer[postion + 2] << 16 | m_buffer[postion + 3] << 24);
        }

        public static long ReadInt64(byte[] m_buffer, int postion)
        {
            uint num = (uint)(m_buffer[postion + 0] | m_buffer[postion + 1] << 8 | m_buffer[postion + 2] << 16 | m_buffer[postion + 3] << 24);
            uint num2 = (uint)(m_buffer[postion + 4] | m_buffer[postion + 5] << 8 | m_buffer[postion + 6] << 16 | m_buffer[postion + 7] << 24);
            return (long)((ulong)num2 << 32 | num);
        }

        public static ulong ReadUInt64(byte[] m_buffer, int postion)
        {
            uint num = (uint)(m_buffer[postion + 0] | m_buffer[postion + 1] << 8 | m_buffer[postion + 2] << 16 | m_buffer[postion + 3] << 24);
            uint num2 = (uint)(m_buffer[postion + 4] | m_buffer[postion + 5] << 8 | m_buffer[postion + 6] << 16 | m_buffer[postion + 7] << 24);
            return (ulong)num2 << 32 | num;
        }
    }
}