using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA2MapNameEncrypt.Utils
{
    class CRC32
    {
        // See source: https://www.cnblogs.com/Kconnie/p/3538194.html

        protected ulong[] Crc32Table;

        public CRC32()
        {
            Crc32Table = new ulong[256];
            for (int i = 0; i < 256; i++)
            {
                var crc = (ulong)i;
                for (int j = 8; j > 0; j--)
                {
                    if ((crc & 1) == 1)
                        crc = (crc >> 1) ^ 0xEDB88320;
                    else
                        crc >>= 1;
                }
                Crc32Table[i] = crc;
            }
        }

        public ulong Encrypt(string sInputString)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(sInputString);
            ulong value = 0xffffffff;
            int len = buffer.Length;
            for (int i = 0; i < len; i++)
            {
                value = (value >> 8) ^ Crc32Table[(value & 0xFF) ^ buffer[i]];
            }
            return value ^ 0xffffffff;
        }
    }
}
