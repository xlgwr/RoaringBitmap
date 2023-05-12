using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections
{

    public class ConcurrentBitLongArrayDic : ConcurrentDictionary<long, BitArray>
    {
        /// <summary>
        /// 低位，低位数
        /// </summary>
        int LowModelValue = int.MaxValue;
        /// <summary>
        /// 低位，低位数是否为int.MaxValue
        /// </summary>
        bool isIntMaxValue = false;

        int hightBit = 31;
        /// <summary>
        /// ((1 << (31))-1)
        /// </summary>
        int lowBitMask = 0xFFF_FFFF;

        public static int concurrevel = Environment.ProcessorCount < 4 ? 2 : Environment.ProcessorCount / 2;
        /// <summary>
        /// 用于判断是否存在
        /// 原理：long的高位做为key,低位为value
        ///  例子：变化范围(int)：0->1_00_000_000 , 
        ///   高位移位：2的指数，向上取整：27=Math.Ceiling(Math.Log(1_00_000_000, 2))
        ///   低位掩码：((1 << 27)-1)
        ///   key:取long值高位，如：120230511_00_000_001 >> 27
        ///   value:取long值低位，如：120230511_00_000_001  & ((1 << 27)-1)
        /// </summary>
        /// <param name="maxValue"></param>
        /// <param name="LowMaxValue">低位值int.MaxValue</param>
        public ConcurrentBitLongArrayDic(int LowMaxValue = int.MaxValue, int Capacity = 2) : base(concurrevel, Capacity)
        {
            this.LowModelValue = LowMaxValue < 65_536 ? 65_536 : LowMaxValue;
            isIntMaxValue = LowMaxValue == int.MaxValue;
            if (!isIntMaxValue)
            {
                hightBit = (int)Math.Ceiling(Math.Log(LowMaxValue, 2));
                lowBitMask = (int)((1 << (hightBit)) - 1);
            }
        }
        #region 基础方法
        /// <summary>
        ///  是否存在,不知高位
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(long index)
        {
            //计算高位
            long getHightIndex = index >> hightBit;

            //是否有高位
            if (!TryGetValue(getHightIndex, out var currBit))
            {
                return false;
            }
            //计算低位
            int lowModelIndex = (int)(index & lowBitMask);
            return currBit.Get(lowModelIndex);
        }
        /// <summary>
        /// 是否存在,已知高位
        /// </summary>
        /// <param name="getHightIndex">高位，</param>
        /// <param name="lowModelIndex"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(long getHightIndex, int lowModelIndex)
        {
            //是否有高位
            if (!TryGetValue(getHightIndex, out var currBit))
            {
                return false;
            }
            return currBit.Get(lowModelIndex);
        }
        /// <summary>
        /// 赋值，不知高位
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(long index, bool value)
        {
            //高位
            long getHightIndex = index >> hightBit;

            //是否存在高位
            if (!TryGetValue(getHightIndex, out var currBit))
            {
                this[getHightIndex] = currBit = new BitArray(lowBitMask + 1);
            }

            //计算低位
            int lowModelIndex = (int)(index & lowBitMask);

            currBit.Set(lowModelIndex, value);
        }

        /// <summary>
        /// 赋值，已知高位
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(long getHightIndex, int lowModelIndex, bool value)
        {
            //是否存在搞位
            if (!TryGetValue(getHightIndex, out var currBit))
            {
                this[getHightIndex] = currBit = new BitArray(lowBitMask + 1);
            }
            currBit.Set(lowModelIndex, value);
        }
        #endregion
        #region state method

        /// <summary>
        /// 计算高位及低位
        /// int,int
        /// HightIndex,LowModelIndex
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static Tuple<long, int> GetHightModelIndex(long index, int modelValue)
        {
            var tmpHightbit = modelValue == int.MaxValue ? 31 : (int)Math.Ceiling(Math.Log(modelValue, 2));

            var lowBitMask = (int)((1 << (tmpHightbit)) - 1);

            long getHightIndex = index >> tmpHightbit;

            int lowModelIndex = (int)(index & lowBitMask);

            return new Tuple<long, int>(getHightIndex, lowModelIndex);
        }
        #endregion
    }



    public class BitLongArrayDic : Dictionary<long, BitArray>
    {
        /// <summary>
        /// 低位，低位数
        /// </summary>
        int LowModelValue = int.MaxValue;
        /// <summary>
        /// 低位，低位数是否为int.MaxValue
        /// </summary>
        bool isIntMaxValue = false;
        /// <summary>
        /// 高位移位
        /// (int)Math.Ceiling(Math.Log(LowModelValue, 2))
        /// </summary>
        int hightBit = 31;

        /// <summary>
        /// ((1 << (31))-1)
        /// 低位掩码，默认0xFFF_FFFF
        /// </summary>
        int lowBitMask = 0xFFF_FFFF;
        /// <summary>
        /// 用于判断是否存在
        /// 原理：long的高位做为key,低位为value
        ///  例子：变化范围(int)：0->1_00_000_000 , 
        ///   高位移位：2的指数，向上取整：27=Math.Ceiling(Math.Log(1_00_000_000, 2))
        ///   低位掩码：((1 << 27)-1)
        ///   key:取long值高位，如：120230511_00_000_001 >> 27
        ///   value:取long值低位，如：120230511_00_000_001  & ((1 << 27)-1)
        /// </summary>
        /// <param name="maxValue"></param>
        /// <param name="LowMaxValue">低位值int.MaxValue,小于65_536，直接为65_536 </param>
        public BitLongArrayDic(int LowMaxValue = int.MaxValue, int Capacity = 2) : base(Capacity)
        {
            this.LowModelValue = LowMaxValue < 65_536 ? 65_536 : LowMaxValue;
            isIntMaxValue = LowMaxValue == int.MaxValue;
            if (!isIntMaxValue)
            {
                hightBit = (int)Math.Ceiling(Math.Log(LowMaxValue, 2));
                lowBitMask = (int)((1 << (hightBit)) - 1);
            }
        }

        #region 基础方法
        /// <summary>
        ///  是否存在,不知高位
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(long index)
        {
            //计算高位
            long getHightIndex = index >> hightBit;

            //是否有高位
            if (!TryGetValue(getHightIndex, out var currBit))
            {
                return false;
            }
            //计算低位
            int lowModelIndex = (int)(index & lowBitMask);
            return currBit.Get(lowModelIndex);
        }
        /// <summary>
        /// 是否存在,已知高位
        /// </summary>
        /// <param name="getHightIndex">高位，</param>
        /// <param name="lowModelIndex"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(long getHightIndex, int lowModelIndex)
        {
            //是否有高位
            if (!TryGetValue(getHightIndex, out var currBit))
            {
                return false;
            }
            return currBit.Get(lowModelIndex);
        }
        /// <summary>
        /// 赋值，不知高位
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(long index, bool value)
        {
            //高位
            long getHightIndex = index >> hightBit;

            //是否存在低位bitarray
            if (!TryGetValue(getHightIndex, out var currBit))
            {
                this[getHightIndex] = currBit = new BitArray(lowBitMask + 1);
            }

            //计算
            var lowModelIndex = (int)(index & lowBitMask);

            currBit.Set(lowModelIndex, value);
            if (value)
            {
                max_bitlength++;
            }
            else
            {
                max_bitlength--;
            }
        }

        /// <summary>
        /// 赋值，已知高位
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(long getHightIndex, int lowModelIndex, bool value)
        {
            //是否存在高位
            if (!TryGetValue(getHightIndex, out var currBit))
            {
                this[getHightIndex] = currBit = new BitArray(lowBitMask + 1);
            }
            currBit.Set(lowModelIndex, value);
            if (value)
            {
                max_bitlength++;
            }
            else
            {
                max_bitlength--;
            }
        }
        long max_bitlength = 0;
        #endregion
        public long GetBitLength
        {
            get { return max_bitlength; }
        }
        #region state method

        /// <summary>
        /// 计算高位及低位
        /// int,int
        /// HightIndex,LowModelIndex
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static Tuple<long, int> GetHightModelIndex(long index, int modelValue)
        {
            var tmpHightbit = modelValue == int.MaxValue ? 31 : (int)Math.Ceiling(Math.Log(modelValue, 2));

            var lowBitMask = (int)((1 << (tmpHightbit)) - 1);

            long getHightIndex = index >> tmpHightbit;

            int lowModelIndex = (int)(index & lowBitMask);

            return new Tuple<long, int>(getHightIndex, lowModelIndex);
        }
        #endregion
    }
}
