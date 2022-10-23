namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var a = Test<int>();
            var b = Test<string>();
            var c = Test<double>();
        }


        public List<T> Test<T>()
        {

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Int32:
                    Console.WriteLine("int");
                    List<int> a = new();
                    a.Add(1);
                    a.Add(2);
                    return (List<T>)(a as object);
                case TypeCode.Double:
                    Console.WriteLine("double");
                    return new List<T>();
                case TypeCode.String:
                    Console.WriteLine("string");
                    return new List<T>();
                default:
                    return new List<T>();
            }
        }


    }
}