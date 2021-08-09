using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RakNetLib.RakData;

namespace RakNetLib.Data
{
    static class Magic
    {
        public static byte[] Value { get => magic; }
        public static bool IsMagic(byte[] data, int ptr) => IsMagic(data, ref ptr);
        public static bool IsMagic(byte[] data, ref int ptr)
        {
            bool Valid = true;
            for (int i = 0; i < 16; i++)
            {
                if (data[ptr + i] != magic[i])
                {
                    Valid = false;
                    break;
                }
            }

            ptr += 16;

            return Valid;
        }

    }
}
