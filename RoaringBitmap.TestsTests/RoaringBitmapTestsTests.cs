using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoaringBitmap.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic.Special;
using Special = System.Collections.Generic.Special;

namespace RoaringBitmap.Tests.Tests
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
            var rb = Special.RoaringBitmap.Create(list, ContainerType.BitmapContainer);
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
    }
}