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
            stopwatch.Stop();
            Msg(title, stopwatch.Elapsed, ",max:", max, ",size:", size);
            stopwatch.Restart();

            var rb = SimpleRoaringBitmap.Create(ContainerType.BitmapContainer, (max >> 16) + 1);
            foreach (var item in list)
            {
                rb.Set(item);
            }

            stopwatch.Stop();
            title = "SimpleRoaringBitmap";
            Msg(title, stopwatch.Elapsed);
            stopwatch.Restart();

            Assert.IsTrue(rb.Contains(65533));
            Assert.IsTrue(rb.Contains(65577));
            Assert.IsTrue(!rb.Contains(65597 * 2));

            stopwatch.Stop();
            Msg($"{title}:Contains", stopwatch.Elapsed);
            stopwatch.Restart();

            var dic = new Dictionary<int, bool>();
            foreach (var item in list)
            {
                dic[item] = true;
            }

            stopwatch.Stop();
            title = "Dictionary";
            Msg(title, stopwatch.Elapsed, "Count:", dic.Count);
            stopwatch.Restart();

            Assert.IsTrue(dic.ContainsKey(65533));
            Assert.IsTrue(dic.ContainsKey(65577));
            Assert.IsTrue(!dic.ContainsKey(65597 * 2));

            stopwatch.Stop();
            Msg($"{title}:ContainsKey", stopwatch.Elapsed); stopwatch.Restart();

            var bitArr = new BitArray(max);
            foreach (var item in list)
            {
                bitArr[item] = true;
            }

            stopwatch.Stop();
            title = "BitArray";
            Msg(title, stopwatch.Elapsed, "Count:", dic.Count);
            stopwatch.Restart();

            Assert.IsTrue(bitArr[65533]);
            Assert.IsTrue(bitArr[65577]);
            Assert.IsTrue(!bitArr[65597 * 2]);

            stopwatch.Stop();
            Msg($"{title}:ContainsKey", stopwatch.Elapsed);
        }
        static void Msg(string msg, TimeSpan timeSpan, params object[] dd)
        {
            Console.WriteLine($"{msg}:{timeSpan},Other:{toStr(dd)}");
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