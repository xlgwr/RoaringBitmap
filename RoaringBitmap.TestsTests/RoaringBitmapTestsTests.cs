using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic.Special;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections;

namespace UnitTesting
{
    [TestClass()]
    public class RoaringBitmapTestsTests
    {
        [TestMethod()]
        public void TestContainsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TestContains()
        {
            var list = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 65533, 65577 };
            var rb = RoaringBitmap.Create(list, ContainerType.BitmapContainer);
            Assert.IsTrue(rb.Contains(65533));
            Assert.IsTrue(rb.Contains(65577));
            Assert.IsTrue(!rb.Contains(65597 * 2));
        }

        [TestMethod()]
        public void TestSetResize()
        {
            var list = new List<int>() { 65536, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 65533, 65577 };
            var rb = SimpleRoaringBitmap.Create(ContainerType.BitmapContainer);
            foreach (var item in list)
            {
                rb.Set(item);
            }
            Assert.IsTrue(rb.Contains(65533));
            Assert.IsTrue(rb.Contains(65577));
            Assert.IsTrue(!rb.Contains(65597 * 2));
        }
        [TestMethod()]
        public void TestRandom()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var max = 100_000_000;
            var size = 2_000_000;
            var random = new Random();
            var list = CreateRandomList(random, size, max);
            list.Add(65533);
            list.Add(65577);

            string title = "GenData";
            Msg(title, stopwatch, ",max:", max, ",size:", size);

            var rb = SimpleRoaringBitmap.Create(ContainerType.SimpleBitmapContainer, (max >> 16) + 1);
            foreach (var item in list)
            {
                rb.Set(item);
            }

            title = "SimpleRoaringBitmap";
            Msg(title, stopwatch);

            Assert.IsTrue(rb.Contains(65533));
            Assert.IsTrue(rb.Contains(65577));
            Assert.IsTrue(!rb.Contains(65597 * 2));

            Msg($"{title}:Contains", stopwatch);

            var dic = new Dictionary<int, bool>();
            foreach (var item in list)
            {
                dic[item] = true;
            }

            title = "Dictionary";
            Msg(title, stopwatch, "Count:", dic.Count);

            Assert.IsTrue(dic.ContainsKey(65533));
            Assert.IsTrue(dic.ContainsKey(65577));
            Assert.IsTrue(!dic.ContainsKey(65597 * 2));

            Msg($"{title}:ContainsKey", stopwatch); stopwatch.Restart();

            title = "BitArray";
            var bitArr = new BitArray(max);
            foreach (var item in list)
            {
                bitArr[item] = true;
            }

            Msg(title, stopwatch, "Count:", bitArr.Count);

            Assert.IsTrue(bitArr[65533]);
            Assert.IsTrue(bitArr[65577]);
            Assert.IsTrue(!bitArr[65597 * 2]);

            Msg($"{title}:ContainsKey", stopwatch);
        }

        [TestMethod()]
        public void TestBitLongArrayDic()
        {

            Stopwatch stopwatch = Stopwatch.StartNew();
            var max1 = 120230510_00_000_000;
            var max2 = 120230511_00_000_001;
            var size = 1_00_000_000 * 1;
            var random = new Random();
            var list = GenLongList(max1, size);
            list.Add(65533);
            list.Add(65577);
            //list.Add(max2);

            //test log 2
            var addValue = 65536;
            Log2(addValue);
            Log2(100000000);

            string title = "GenData";
            Msg(title, stopwatch, ",max:", max1, ",size:", size);

            title = "BitLongArrayDic";
            var bitArr = new BitLongArrayDic(100000000, 2);
            foreach (var item in list)
            {
                bitArr.Set(item, true);
            }

            Msg(title, stopwatch, "Count:", bitArr.Count);

            Assert.IsTrue(bitArr.Get(65533));
            Assert.IsTrue(bitArr.Get(65577));
            Assert.IsTrue(!bitArr.Get(65597 * 2));

            Assert.IsTrue(bitArr.Get(max1 + 1));
            Assert.IsTrue(bitArr.Get(max2));

            Msg($"{title}:ContainsKey", stopwatch);

            title = "Dictionary";
            var dic = new Dictionary<long, bool>(size);
            foreach (var item in list)
            {
                dic[item] = true;
            }

            Msg(title, stopwatch, "Count:", dic.Count);

            Assert.IsTrue(dic.ContainsKey(65533));
            Assert.IsTrue(dic.ContainsKey(65577));
            Assert.IsTrue(!dic.ContainsKey(65597 * 2));

            Msg($"{title}:ContainsKey", stopwatch); stopwatch.Restart();
        }
        #region time log msg
        static void Log2(int value)
        {
            var dd = Math.Log(value, 2);
            Console.WriteLine($"{dd}:Round:{Math.Round(dd)},Floor:{Math.Floor(dd)}");
        }
        static void Msg(string msg, Stopwatch stopwatch, params object[] dd)
        {
            stopwatch.Stop();
            Console.WriteLine($"{msg}:{stopwatch.Elapsed},Other:{toStr(dd)}");
            stopwatch.Restart();

        }
        static string toStr(object[] dd)
        {
            string str = string.Empty;
            foreach (var item in dd)
            {
                str += item.ToString();
            }
            return str;
        }
        #endregion
        #region other method
        private static List<long> GenLongList(long minValue, int size = 1_000_000)
        {
            var result = new List<long>(size);
            for (int i = 0; i < size; i++)
            {
                result.Add(minValue + i);
            }
            return result;
        }
        private static List<int> CreateRandomList(Random random, int size, int maxValue = int.MaxValue)
        {
            if (maxValue < size)
            {
                maxValue = size + 1;
            }
            var list = new List<int>(size);
            var type = random.Next() % 2;
            if (type == 0)
            {
                for (var i = 0; i < size; i++)
                {
                    list.Add(random.Next(0, maxValue));
                }
            }
            else
            {
                var start = random.Next(0, maxValue - size);
                for (var i = start; i < start + size; i++)
                {
                    list.Add(i);
                }
            }
            return list;
        }
        #endregion
    }
}