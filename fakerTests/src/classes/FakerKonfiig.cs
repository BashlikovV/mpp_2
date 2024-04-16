using faker;

namespace fakerTests
{
    public class TestClass1
    {
        public int f1;

        public TestClass2 f2;
    }

    public class TestClass2
    {
        public int f1;
        public int f2;
        public string f3;

        public TestClass1 f4;

        public int proc()
        {
            return 9;
        }
    }

    public class GenInt : ICustomGenerator<int>
    {
        public int generate()
        {
            return 9;
        }
    }

    public class GenString : ICustomGenerator<string>
    {
        public string generate()
        {
            return "9";
        }
    }

    public class GenTestClass1 : ICustomGenerator<TestClass1>
    {
        public TestClass1 generate()
        {
            return new TestClass1
            {
                f1 = 9,
                f2 = null
            };
        }
    }

    public class RecursiveClass1
    {
        public RecursiveClass2 cycled;
    }

    public class RecursiveClass2
    {
        public RecursiveClass1 test;
    }

    public class ExampleClass
    {
        public float field = float.NaN;

        public float property { get; set; } = float.NaN;

        public List<List<ExampleClassInner>> list;
    }

    public class ExampleClassInner
    {
        public float field = float.NaN;
        public float property = float.NaN;
    }
}
