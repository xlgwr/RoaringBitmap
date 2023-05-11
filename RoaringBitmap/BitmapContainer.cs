using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic.Special
{
    internal class BitmapContainer : SimpleBitmapContainer
    {
        public static readonly BitmapContainer One;
        static BitmapContainer()
        {
            var data = new ulong[BitmapLength];
            for (var i = 0; i < BitmapLength; i++)
            {
                data[i] = ulong.MaxValue;
            }
            One = new BitmapContainer(1 << 16, data);
        }
        public BitmapContainer(ushort v) : base(v) { }

        private BitmapContainer(int cardinality) : base(0, cardinality)
        {
        }

        private BitmapContainer(int cardinality, ulong[] data) : base(cardinality, data)
        {

        }

        private BitmapContainer(int cardinality, ushort[] values, bool negated) : this(negated ? MaxCapacity - cardinality : cardinality)
        {
            if (negated)
            {
                for (var i = 0; i < BitmapLength; i++)
                {
                    m_Bitmap[i] = ulong.MaxValue;
                }
                for (var i = 0; i < cardinality; i++)
                {
                    var v = values[i];
                    m_Bitmap[v >> 6] &= ~(1UL << v);
                }
            }
            else
            {
                for (var i = 0; i < cardinality; i++)
                {
                    var v = values[i];
                    m_Bitmap[v >> 6] |= 1UL << v;
                }
            }
        }

        internal static BitmapContainer Create(ushort[] values)
        {
            return new BitmapContainer(values.Length, values, false);
        }

        internal static BitmapContainer Create(int cardinality, ushort[] values)
        {
            return new BitmapContainer(cardinality, values, false);
        }

        internal static BitmapContainer Create(int cardinality, ushort[] values, bool negated)
        {
            return new BitmapContainer(cardinality, values, negated);
        }


        internal static BitmapContainer CreateXor(ushort[] first, int firstCardinality, ushort[] second, int secondCardinality)
        {
            var data = new ulong[BitmapLength];
            for (var i = 0; i < firstCardinality; i++)
            {
                var v = first[i];
                data[v >> 6] ^= 1UL << v;
            }

            for (var i = 0; i < secondCardinality; i++)
            {
                var v = second[i];
                data[v >> 6] ^= 1UL << v;
            }
            var cardinality = Util.BitCount(data);
            return new BitmapContainer(cardinality, data);
        }

        /// <summary>
        ///     Java version has an optimized version of this, but it's using bitcount internally which should make it slower in
        ///     .NET
        /// </summary>
        public static Container operator &(BitmapContainer x, BitmapContainer y)
        {
            var data = Clone(x.m_Bitmap);
            var bc = new BitmapContainer(AndInternal(data, y.m_Bitmap), data);
            return bc.m_Cardinality <= MaxSize ? (Container)ArrayContainer.Create(bc) : bc;
        }

        private static ulong[] Clone(ulong[] data)
        {
            var result = new ulong[BitmapLength];
            Buffer.BlockCopy(data, 0, result, 0, BitmapLength * sizeof(ulong));
            return result;
        }

        public static ArrayContainer operator &(BitmapContainer x, ArrayContainer y)
        {
            return y & x;
        }

        public static BitmapContainer operator |(BitmapContainer x, BitmapContainer y)
        {
            var data = Clone(x.m_Bitmap);
            return new BitmapContainer(OrInternal(data, y.m_Bitmap), data);
        }

        public static BitmapContainer operator |(BitmapContainer x, ArrayContainer y)
        {
            var data = Clone(x.m_Bitmap);
            return new BitmapContainer(x.m_Cardinality + y.OrArray(data), data);
        }

        public static Container operator ~(BitmapContainer x)
        {
            var data = Clone(x.m_Bitmap);
            var bc = new BitmapContainer(NotInternal(data), data);
            return bc.m_Cardinality <= MaxSize ? (Container)ArrayContainer.Create(bc) : bc;
        }

        /// <summary>
        ///     Java version has an optimized version of this, but it's using bitcount internally which should make it slower in
        ///     .NET
        /// </summary>
        public static Container operator ^(BitmapContainer x, BitmapContainer y)
        {
            var data = Clone(x.m_Bitmap);
            var bc = new BitmapContainer(XorInternal(data, y.m_Bitmap), data);
            return bc.m_Cardinality <= MaxSize ? (Container)ArrayContainer.Create(bc) : bc;
        }


        public static Container operator ^(BitmapContainer x, ArrayContainer y)
        {
            var data = Clone(x.m_Bitmap);
            var bc = new BitmapContainer(x.m_Cardinality + y.XorArray(data), data);
            return bc.m_Cardinality <= MaxSize ? (Container)ArrayContainer.Create(bc) : bc;
        }

        public static Container AndNot(BitmapContainer x, BitmapContainer y)
        {
            var data = Clone(x.m_Bitmap);
            var bc = new BitmapContainer(AndNotInternal(data, y.m_Bitmap), data);
            return bc.m_Cardinality <= MaxSize ? (Container)ArrayContainer.Create(bc) : bc;
        }

        public static Container AndNot(BitmapContainer x, ArrayContainer y)
        {
            var data = Clone(x.m_Bitmap);
            var bc = new BitmapContainer(x.m_Cardinality + y.AndNotArray(data), data);
            return bc.m_Cardinality <= MaxSize ? (Container)ArrayContainer.Create(bc) : bc;
        }

        private static int XorInternal(ulong[] first, ulong[] second)
        {
            for (var k = 0; k < BitmapLength; k++)
            {
                first[k] = first[k] ^ second[k];
            }
            var c = Util.BitCount(first);
            return c;
        }

        private static int AndNotInternal(ulong[] first, ulong[] second)
        {
            for (var k = 0; k < first.Length; k++)
            {
                first[k] = first[k] & ~second[k];
            }
            var c = Util.BitCount(first);
            return c;
        }

        private static int NotInternal(ulong[] data)
        {
            for (var k = 0; k < BitmapLength; k++)
            {
                data[k] = ~data[k];
            }
            var c = Util.BitCount(data);
            return c;
        }

        private static int OrInternal(ulong[] first, ulong[] second)
        {
            for (var k = 0; k < BitmapLength; k++)
            {
                first[k] = first[k] | second[k];
            }
            var c = Util.BitCount(first);
            return c;
        }

        private static int AndInternal(ulong[] first, ulong[] second)
        {
            for (var k = 0; k < BitmapLength; k++)
            {
                first[k] = first[k] & second[k];
            }
            var c = Util.BitCount(first);
            return c;
        }

        internal int FillArray(ushort[] data)
        {
            var pos = 0;
            for (var k = 0; k < BitmapLength; k++)
            {
                var bitset = m_Bitmap[k];
                var shiftedK = k << 6;
                while (bitset != 0)
                {
                    var t = bitset & (~bitset + 1);
                    data[pos++] = (ushort)(shiftedK + Util.BitCount(t - 1));
                    bitset ^= t;
                }
            }
            return m_Cardinality;
        }



        public static void Serialize(BitmapContainer bc, BinaryWriter binaryWriter)
        {
            for (var i = 0; i < BitmapLength; i++)
            {
                binaryWriter.Write(bc.m_Bitmap[i]);
            }
        }

        public static BitmapContainer Deserialize(BinaryReader binaryReader, int cardinality)
        {
            var data = new ulong[BitmapLength];
            for (var i = 0; i < BitmapLength; i++)
            {
                data[i] = binaryReader.ReadUInt64();
            }
            return new BitmapContainer(cardinality, data);
        }


    }
}