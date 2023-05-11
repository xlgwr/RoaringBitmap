using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace System.Collections.Generic.Special
{
    public class SimpleBitmapContainer : Container, IEquatable<SimpleBitmapContainer>
    {
        public const int BitmapLength = 1024;
        protected ulong[] m_Bitmap;
        protected int m_Cardinality;


        protected internal override int Cardinality => m_Cardinality;
        public override int ArraySizeInBytes => MaxCapacity / 8;

        public SimpleBitmapContainer(ushort v, int card = 1)
        {
            m_Bitmap = new ulong[BitmapLength];
            m_Cardinality = card;
            if (v > 0)
            {
                m_Bitmap[v >> 6] = 1UL << v;
            }
        }
        public SimpleBitmapContainer(int cardinality, ulong[] data)
        {
            m_Bitmap = data;
            m_Cardinality = cardinality;
        }

        public static SimpleBitmapContainer Create(ushort v)
        {
            return new SimpleBitmapContainer(v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Get(ushort x)
        {
            return (m_Bitmap[x >> 6] & (1UL << x)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Set(ushort v, bool value = true)
        {
            //值
            if (value)
            {
                m_Bitmap[v >> 6] |= 1UL << v;
                m_Cardinality++;
            }
            else
            {
                m_Bitmap[v >> 6] ^= 1UL << v;
                m_Cardinality--;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Contains(ushort x)
        {
            return Contains(m_Bitmap, x);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Contains(ulong[] bitmap, ushort x)
        {
            return (bitmap[x >> 6] & (1UL << x)) != 0;
        }
        #region interface
        public bool Equals(SimpleBitmapContainer other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (m_Cardinality != other.m_Cardinality)
            {
                return false;
            }
            for (var i = 0; i < BitmapLength; i++)
            {
                if (m_Bitmap[i] != other.m_Bitmap[i])
                {
                    return false;
                }
            }
            return true;
        }
        #region over
        protected override bool EqualsInternal(Container other)
        {
            var bc = other as BitmapContainer;
            return (bc != null) && Equals(bc);
        }

        public override IEnumerator<ushort> GetEnumerator()
        {
            for (var k = 0; k < BitmapLength; k++)
            {
                var bitset = m_Bitmap[k];
                var shiftedK = k << 6;
                while (bitset != 0)
                {
                    var t = bitset & (~bitset + 1);
                    var result = (ushort)(shiftedK + Util.BitCount(t - 1));
                    yield return result;
                    bitset ^= t;
                }
            }
        }
        public override bool Equals(object obj)
        {
            var bc = obj as BitmapContainer;
            return (bc != null) && Equals(bc);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var code = 17;
                code = code * 23 + m_Cardinality;
                for (var i = 0; i < BitmapLength; i++)
                {
                    code = code * 23 + m_Bitmap[i].GetHashCode();
                }
                return code;
            }
        }
        #endregion
        #endregion
    }

}
