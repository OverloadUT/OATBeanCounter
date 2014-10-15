using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OATBeanCounter
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            BCDataStorage data = OATBeanCounterData.data;
            data.OnEncodeToConfigNode();
            Assert.AreEqual(data.data_version, "0.01");
        }
    }
}
