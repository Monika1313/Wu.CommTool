using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Wu.CommTool.Models;
using Wu.Extensions;

namespace Test
{
    /// <summary>
    /// ModbusRtuFrame类测试
    /// </summary>
    [TestClass]
    public class ModbusRtuFrameTest
    {
        [TestMethod]
        public void TestToString()
        {
            //测试03请求帧格式化
            byte[] f1 = "010300000001840A".GetBytes();
            Assert.AreEqual(new ModbusRtuFrame(f1).GetFormatFrame(), "01 03 0000 0001 840A");
            byte[] f11 = "01030BCE0002A7D0".GetBytes();
            Assert.AreEqual(new ModbusRtuFrame(f11).GetFormatFrame(), "01 03 0BCE 0002 A7D0");

            //测试03应答帧格式化
            byte[] f2 = "0103044005F16CBA4F".GetBytes();
            Assert.AreEqual(new ModbusRtuFrame(f2).GetFormatFrame(), "01 03 04 4005 F16C BA4F");

        }
    }
}