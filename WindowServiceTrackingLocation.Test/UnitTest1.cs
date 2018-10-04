using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsServiceTrackingLocation;

namespace WindowServiceTrackingLocation.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Utilities utils = new Utilities();
            bool result = utils.SendingMailUsingMailKit();
            Assert.AreEqual(true, result);
        }

        static void Main()
        {
            
        }
    }
}
