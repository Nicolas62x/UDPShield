using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakNetLib.Data
{
    static class Short
    {
        public static short GetShort(byte[] data, int ptr) => GetShort(data, ref ptr);
        public static short GetShort(byte[] data, ref int ptr) => (short)
            ((data[ptr++] << 08) |
            (data[ptr++] << 00));

        public static void AddShort(in List<byte> data, short value)
        {
            for (int i = 1; i >= 0; i--)
            {
                data.Add((byte)((ushort)value >> i * 8 & 0xff));
            }
        }

        public static void SetShortAt(in List<byte> data, int offset, short value)
        {
            for (int i = 1; i >= 0; i--)
            {
                data[offset + 1 - i] = ((byte)((ushort)value >> i * 8 & 0xff));
            }
        }
    }
}
