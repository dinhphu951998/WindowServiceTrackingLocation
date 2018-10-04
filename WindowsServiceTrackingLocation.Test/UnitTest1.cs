using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WindowsServiceTrackingLocation.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethodCheckInternetConnection()
        {
            WindowServiceTrackingLocation w = new WindowServiceTrackingLocation();
            var result = w.CheckInternetConnection();
            Assert.AreEqual(true, result);
        }

        //[TestMethod]
        //public void TestWriteAddressFile()
        //{
        //    Utilities util = new Utilities();
        //    util.LogAddress("address", "addresscomponent");
        //}
    }
}
