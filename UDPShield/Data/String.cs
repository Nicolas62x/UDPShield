using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakNetLib.Data
{
    static class String
    {
        public static string GetString(byte[] data, int ptr) => GetString(data, ref ptr);
        public static string GetString(byte[] data, ref int ptr)
        {
            ushort len = (ushort)Short.GetShort(data, ref ptr);
            return Encoding.UTF8.GetString(data, ptr+=len, len);
        }

        public static void AddString(in List<byte> data, string value)
        {
            byte[] dat = Encoding.UTF8.GetBytes(value);
            Short.AddShort(data, (short)dat.Length);
            data.AddRange(dat);
        }
    }
}
