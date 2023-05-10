using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic.Special
{
    public class SimpleRoaringBitmap
    {
        
        private readonly SimpleRoaringArray m_HighLowContainer;

        public SimpleRoaringBitmap(SimpleRoaringArray input)
        {
            m_HighLowContainer = input;
        } 

        public static SimpleRoaringBitmap Create(ContainerType containerType, int size = 0)
        {
            return new SimpleRoaringBitmap(new SimpleRoaringArray(containerType, size));
        }

        #region Get Set
        public bool Contains(int v, int hightIndex = -1)
        {
            return m_HighLowContainer.Contains(v, hightIndex);
        }
        /// <summary>
        /// 是否存在,
        /// </summary>
        /// <param name="v">查询值</param>
        /// <param name="hightIndex">高位索引</param>
        /// <returns></returns>
        public bool Get(int v, int hightIndex = -1)
        {
            return m_HighLowContainer.Contains(v, hightIndex);
        }
        public void Set(int v)
        {
            m_HighLowContainer.Set(v);
        }
        #endregion
    }
}
