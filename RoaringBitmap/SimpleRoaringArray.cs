using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic.Special
{
    public class SimpleRoaringArray
    {
        protected ushort[] m_Keys;
        protected int m_Size;
        protected Container[] m_Values;
        protected readonly ContainerType containerType;
        public long Cardinality { get; }

        public SimpleRoaringArray(ContainerType containerType, int size = 0)
        {
            this.containerType = containerType;
            m_Size = size;
            m_Keys = new ushort[m_Size];
            m_Values = new Container[m_Size];
            if (m_Size > 0)
            {
                for (int i = 0; i < m_Size; i++)
                {
                    m_Keys[i] = (ushort)i;
                    m_Values[i] = NewContainer(0);
                }
            }
        }

        public SimpleRoaringArray(int size, ushort[] keys, Container[] containers)
        {
            m_Size = size;
            m_Keys = keys;
            m_Values = containers;
            for (var i = 0; i < containers.Length; i++)
            {
                Cardinality += containers[i].Cardinality;
            }
        }

        private int GetIndex(ushort key, int index = 0)
        {
            return m_Keys.GetIndex(key, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(int v, int hightIndex = -1)
        {
            var tmpLowHighBits = v.LowHighBits();
            var index = hightIndex > 0 ? hightIndex : GetIndex(tmpLowHighBits.Item2);
            if (index < 0)
            {
                return false;
            }
            return m_Values[index].Contains(tmpLowHighBits.Item1);
        }

        #region Resize
        /// <summary>
        /// 设置,动态扩展
        /// </summary>
        /// <param name="v"></param>
        /// <returns>返回高位index</returns>
        public int Set(int v)
        {
            var tmpLowHighBits = v.LowHighBits();
            //hight
            var index = GetIndex(tmpLowHighBits.Item2);
            if (index < 0)
            {
                //resize
                return Resize(tmpLowHighBits.Item1, tmpLowHighBits.Item2);
            }
            //符值 low
            m_Values[index].Set(tmpLowHighBits.Item1);
            return index;
        }
        Container NewContainer(ushort low = 0)
        {
            #region Container
            Container newContainer;
            switch (this.containerType)
            {
                case ContainerType.SimpleBitmapContainer:
                    newContainer = SimpleBitmapContainer.Create(low);
                    break;
                case ContainerType.BitmapContainer:
                    newContainer = BitmapContainer.Create(low);
                    break;
                case ContainerType.ArrayContainer:
                    newContainer = ArrayContainer.Create(low);
                    break;
                default:
                    newContainer = SimpleBitmapContainer.Create(low);
                    break;
            }
            #endregion
            return newContainer;
        }
        int Resize(ushort low, ushort h)
        {
            int index = m_Keys.Length; ;
            //copy key
            ushort[] copy_Keys = new ushort[index + 1];
            //copy value
            Container[] copy_value = new Container[index + 1];

            //copy to
            if (index > 0)
            {
                m_Keys.CopyTo(copy_Keys, 0);
                m_Values.CopyTo(copy_value, 0);
            }

            Container newContainer = NewContainer(low);

            //set HighBits
            copy_Keys[index] = h;
            //setValue
            copy_value[index] = newContainer;

            //更新引用
            m_Keys = copy_Keys;
            m_Values = copy_value;

            //set null copy
            copy_Keys = null;
            copy_value = null;

            //set size
            m_Size++;

            return index;
        }
        #endregion
    }
}
