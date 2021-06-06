using RA2MapNameEncrypt.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA2MapNameEncrypt.Utils
{
    class BarCode
    {
        // Thanks to SECSOME,
        // and the ideas of Uranisian.
        private BitSet checker;
        private Random rand;
        public BarCode()
        {
            checker = new BitSet(2 << 16);
            rand = new Random();
        }
        private int GetRandom(int start = 0, int end = 2 << 16)
        {
            int counter = 0;
            if (++counter > (2 << 6))
            {
                int seed = (int)DateTime.Now.Ticks % 100;
                rand = new Random(seed);
                counter = 0;
            }
            int dis = end - start;
            return rand.Next() % dis + start;
        }
        public char[] Generate()
        {
            int val;
            do
            {
                val = GetRandom();
            } while (checker[val]);

            checker[val] = true;

            char[] ret = new char[16];
            for (int i = 0; i < 16; ++i)
            {
                ret[i] = (val & (1 << i)) != 0 ? 'i' : 'l';
            }
            return ret;
        }
    }
}
