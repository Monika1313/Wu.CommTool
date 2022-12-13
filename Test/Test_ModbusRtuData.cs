using Wu.CommTool.Models;

namespace Test
{
    [TestClass]
    public class Test_ModbusRtuData
    {
        [TestMethod]
        public void TestMethod1()
        {
            byte[] byte4 = { 1, 2, 3, 4};
            byte[] byte8 = { 1, 2, 3, 4, 5, 6, 7, 8, };

            ModbusRtuData data = new ModbusRtuData();

        }
    }
}