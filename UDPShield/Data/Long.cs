using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakNetLib.Data
{
    static class Long
    {
        public static long GetLong(byte[] data, int ptr) => GetLong(data,ref ptr);
        public static long GetLong(byte[] data, ref int ptr) => (long)(
            ((ulong)data[ptr ++] << 56) |
            ((ulong)data[ptr ++] << 48) |
            ((ulong)data[ptr ++] << 40) |
            ((ulong)data[ptr ++] << 32) |
            ((ulong)data[ptr ++] << 24) |
            ((ulong)data[ptr ++] << 16) |
            ((ulong)data[ptr ++] << 08) |
            ((ulong)data[ptr ++] << 00));

        public static void AddLong(in List<byte> data, long value)
        {
            for (int i = 0; i < 8; i++)
            {
                data.Add((byte)((ulong)value >> i * 8 & 0xff));
            }
        }
    }
}
