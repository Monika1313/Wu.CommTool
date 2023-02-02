namespace Test
{
    [TestClass]
    public class UnitTest1
    {



        [FlagsAttribute]
        enum Colors_1
        {
            Red = 1, Green = 2, Blue = 4, Yellow = 8
        }


        [TestMethod]
        public void TestMethod1()
        {
            Colors_1 color_2 = Colors_1.Red | Colors_1.Green | Colors_1.Blue| Colors_1.Yellow;

            string strResult = color_2.ToString();
        }




        #region switch 数据类型
        //[TestMethod]
        //public void TestMethod1()
        //{
        //    var a = Test<int>();
        //    var b = Test<string>();
        //    var c = Test<double>();
        //}


        //public List<T> Test<T>()
        //{

        //    switch (Type.GetTypeCode(typeof(T)))
        //    {
        //        case TypeCode.Int32:
        //            Console.WriteLine("int");
        //            List<int> a = new();
        //            a.Add(1);
        //            a.Add(2);
        //            return (List<T>)(a as object);
        //        case TypeCode.Double:
        //            Console.WriteLine("double");
        //            return new List<T>();
        //        case TypeCode.String:
        //            Console.WriteLine("string");
        //            return new List<T>();
        //        default:
        //            return new List<T>();
        //    }
        //} 
        #endregion


    }
}