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
    /// <summary>
    /// BitArray扩展long
    /// int32,8*4 32位 正整数：2^31-1=2,147,483,647 Int32.MaxValue 10位
    /// long,16*4 64位 正整数：2^63-1=9,223,372,036,854,775,807 long.MaxValue 19位
    /// 分界值int32位最大正数：如上
    /// 高位：右移31位 L >> 31
    /// 求模值BitArray:L % Int32.MaxValue
    /// </summary>
    public class ConcurrentBitLongArrayDic : ConcurrentDictionary<long, BitArray>
    {
        /// <summary>
        /// 低位，模数
        /// </summary>
        int LowModelValue = int.MaxValue;
        /// <summary>
        /// 低位，模数是否为int.MaxValue
        /// </summary>
        bool isIntMaxValue = false;

        int LowModelLog2 = 31;

        public static int concurrevel = Environment.ProcessorCount < 4 ? 2 : Environment.ProcessorCount / 2;
        /// <summary>
        /// 
        /// 用于判断是否存在
        /// 
        /// 初始化，长度 右移 31 得高位倍数值
        /// 模值=2,147,483,647 Int32.MaxValue
        /// 如:
        /// 最大长度为17位十进制：19,999,050,800,000,001 >> 31 = 9,312,783
        /// or
        /// 模值=100,000,000
        /// 最大长度17位十进制：19,999,050,800,000,001 / 100,000,000 = 19,999,0508
        /// </summary>
        /// <param name="maxValue"></param>
        /// <param name="LowMaxValue">模值int.MaxValue</param>
        public ConcurrentBitLongArrayDic(int LowMaxValue = int.MaxValue, int Capacity = 2) : base(concurrevel, Capacity)
        {
            this.LowModelValue = LowMaxValue < 65_536 ? 65_536 : LowMaxValue;
            isIntMaxValue = LowMaxValue == int.MaxValue;
            if (!isIntMaxValue)
            {
                LowModelLog2 = (int)Math.Round(Math.Log(LowMaxValue, 2));
            }
        }
        #region 倍数基础方法
        /// <summary>
        ///  是否存在,不知高位倍数
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(long index)
        {
            //计算高位倍数
            long getHightIndex = index >> LowModelLog2;

            //是否有高位
            if (!TryGetValue(getHightIndex, out var currBit))
            {
                return false;
            }
            //计算模
            int lowModelIndex = (int)(index % LowModelValue);
            return currBit.Get(lowModelIndex);
        }
        /// <summary>
        /// 是否存在,已知高位倍数
        /// </summary>
        /// <param name="getHightIndex">高位，倍数</param>
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
        /// 赋值，不知高位倍数
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(long index, bool value)
        {
            //高位/倍数
            long getHightIndex = index >> LowModelLog2;

            //是否存在低位bitarray
            if (!TryGetValue(getHightIndex, out var currBit))
            {
                this[getHightIndex] = currBit = new BitArray(LowModelValue);
            }

            //计算模
            int lowModelIndex = (int)(index % LowModelValue);

            currBit.Set(lowModelIndex, value);
        }

        /// <summary>
        /// 赋值，已知高位倍数
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(long getHightIndex, int lowModelIndex, bool value)
        {
            //是否存在低位bitarray
            if (!TryGetValue(getHightIndex, out var currBit))
            {
                this[getHightIndex] = currBit = new BitArray(LowModelValue);
            }
            currBit.Set(lowModelIndex, value);
        }
        #endregion
        #region state method

        /// <summary>
        /// 计算倍数，及模
        /// int,int
        /// HightIndex,LowModelIndex
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static Tuple<long, int> GetHightModelIndex(long index, int modelValue)
        {
            var tmplog2 = modelValue == int.MaxValue ? 31 : (int)Math.Round(Math.Log(modelValue, 2));

            long getHightIndex = index >> tmplog2;

            int lowModelIndex = (int)(index % modelValue);

            return new Tuple<long, int>(getHightIndex, lowModelIndex);
        }
        #endregion
    }



    public class BitLongArrayDic : Dictionary<long, BitArray>
    {
        /// <summary>
        /// 低位，模数
        /// </summary>
        int LowModelValue = int.MaxValue;
        /// <summary>
        /// 低位，模数是否为int.MaxValue
        /// </summary>
        bool isIntMaxValue = false;

        int LowModelLog2 = 31;
        /// <summary>
        /// 
        /// 用于判断是否存在
        /// 
        /// 初始化，长度 右移 31 得高位倍数值
        /// 模值=2,147,483,647 Int32.MaxValue
        /// 如:
        /// 最大长度为17位十进制：19,999,050,800,000,001 >> 31 = 9,312,783
        /// or
        /// 模值=100,000,000
        /// 最大长度17位十进制：19,999,050,800,000,001 / 100,000,000 = 19,999,0508
        /// </summary>
        /// <param name="maxValue"></param>
        /// <param name="LowMaxValue">模值int.MaxValue,小于65_536，直接为65_536 </param>
        public BitLongArrayDic(int LowMaxValue = int.MaxValue, int Capacity = 2) : base(Capacity)
        {
            this.LowModelValue = LowMaxValue < 65_536 ? 65_536 : LowMaxValue;
            isIntMaxValue = LowMaxValue == int.MaxValue;
            if (!isIntMaxValue)
            {
                LowModelLog2 = (int)Math.Round(Math.Log(LowMaxValue, 2));
            }
        }  

        #region 倍数基础方法
        /// <summary>
        ///  是否存在,不知高位倍数
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(long index)
        {
            //计算高位倍数
            long getHightIndex = index >> LowModelLog2;

            //是否有高位
            if (!TryGetValue(getHightIndex, out var currBit))
            {
                return false;
            }
            //计算模
            int lowModelIndex = (int)(index % LowModelValue);
            return currBit.Get(lowModelIndex);
        }
        /// <summary>
        /// 是否存在,已知高位倍数
        /// </summary>
        /// <param name="getHightIndex">高位，倍数</param>
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
        /// 赋值，不知高位倍数
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(long index, bool value)
        {
            //高位/倍数
            long getHightIndex = index >> LowModelLog2;

            //是否存在低位bitarray
            if (!TryGetValue(getHightIndex, out var currBit))
            {
                this[getHightIndex] = currBit = new BitArray(LowModelValue);
            }

            //计算模
            int lowModelIndex = (int)(index % LowModelValue);

            currBit.Set(lowModelIndex, value);
        }

        /// <summary>
        /// 赋值，已知高位倍数
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(long getHightIndex, int lowModelIndex, bool value)
        {
            //是否存在低位bitarray
            if (!TryGetValue(getHightIndex, out var currBit))
            {
                this[getHightIndex] = currBit = new BitArray(LowModelValue);
            }
            currBit.Set(lowModelIndex, value);
        }
        #endregion
        #region state method

        /// <summary>
        /// 计算倍数，及模
        /// int,int
        /// HightIndex,LowModelIndex
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static Tuple<long, int> GetHightModelIndex(long index, int modelValue)
        {
            var tmplog2 = modelValue == int.MaxValue ? 31 : (int)Math.Round(Math.Log(modelValue, 2));

            long getHightIndex = index >> tmplog2;

            int lowModelIndex = (int)(index % modelValue);

            return new Tuple<long, int>(getHightIndex, lowModelIndex);
        }
        #endregion
    }
}
