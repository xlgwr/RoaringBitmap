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

            var bitArr = new BitArray(max);
            foreach (var item in list)
            {
                bitArr[item] = true;
            }

            title = "BitArray";
            Msg(title, stopwatch, "Count:", bitArr.Count);

            Assert.IsTrue(bitArr[65533]);
            Assert.IsTrue(bitArr[65577]);
            Assert.IsTrue(!bitArr[65597 * 2]);

            Msg($"{title}:ContainsKey", stopwatch);
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
        #region other method
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