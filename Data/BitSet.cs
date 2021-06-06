using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA2MapNameEncrypt.Data
{
    class BitSet
    {
        // See source: https://blog.csdn.net/forward_huan/article/details/107415292

        public int count;
        private bool[] bitset;

        public BitSet(int count)
        {
            this.count = count;
            bitset = new bool[count];
        }

        public bool this[int index]
        {
            get
            {
                CheckIndexLegal(index);
                return bitset[index];
            }
            set
            {
                CheckIndexLegal(index);
                bitset[index] = value;
            }
        }

        public int Size()
        {
            return count;
        }

        public BitSet Clone()
        {
            BitSet bit = new BitSet(count);
            for (int index = 0; index < count; index++)
            {
                bit[index] = this[index];
            }
            return bit;
        }

        public void Set(int index, bool value = true)
        {
            CheckIndexLegal(index);
            this[index] = value;
        }

        public void Set(int startIndex, int endIndex, bool value = true)
        {
            CheckIndexLegal(startIndex);
            CheckIndexLegal(endIndex);
            for (int index = startIndex; index < endIndex; index++)
            {
                this[index] = value;
            }
        }

        public void Clear()
        {
            for (int index = 0; index < count; index++)
            {
                this[index] = false;
            }
        }

        public void Clear(int index)
        {
            CheckIndexLegal(index);
            this[index] = false;
        }

        public void Clear(int startIndex, int endIndex)
        {
            CheckIndexLegal(startIndex);
            CheckIndexLegal(endIndex);
            for (int index = startIndex; index < endIndex; index++)
            {
                this[index] = false;
            }
        }

        public bool Equals(BitSet bit)
        {
            if (this.count == bit.count)
            {
                for (int index = 0; index < this.count; index++)
                {
                    if (this[index] != bit[index])
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        public bool Get(int index)
        {
            CheckIndexLegal(index);
            return this[index];
        }

        public BitSet Get(int startIndex, int endIndex)
        {
            CheckIndexLegal(startIndex);
            CheckIndexLegal(endIndex);
            var length = endIndex - startIndex;
            BitSet bit = new BitSet(length);
            for (int index = 0; index < length; index++)
            {
                bit[index] = this[index + startIndex];
            }
            return bit;
        }

        public bool IsEmpty()
        {
            for (int index = 0; index < count; index++)
            {
                if (this[index])
                {
                    return false;
                }
            }
            return true;
        }

        private void CheckIndexLegal(int index)
        {
            if (index < 0 || index >= count)
            {
                throw new IndexOutOfRangeException();
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int index = count - 1; index >= 0; index--)
            {
                builder.Append((this[index] ? 1 : 0));
            }
            return builder.ToString();
        }

        public int ToInt()
        {
            return Convert.ToInt16(this.ToString(), 2);
        }
    }
}
