using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA2MapNameEncrypt.Utils
{
    static class CRC32
    {
        // See source: https://www.cnblogs.com/Kconnie/p/3538194.html

        private static ulong[] TableInit()
        {
            var ret = new ulong[256];
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
                ret[i] = crc;
            }
            return ret;
        }

        public static ulong Encrypt(string sInputString)
        {
            var Crc32Table = TableInit();
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
