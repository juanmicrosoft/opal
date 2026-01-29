namespace Lambda
{
    using System;

    public static class Lambdas
    {
        public static Func<int, int> GetDoubler()
        {
            return x => x * 2;
        }

        public static Func<int, int, int> GetAdder()
        {
            return (a, b) => a + b;
        }

        public static Func<int, bool> GetIsPositive()
        {
            return x => x > 0;
        }

        public static Action<string> GetPrinter()
        {
            return msg => Console.WriteLine(msg);
        }

        public static int Apply(Func<int, int> f, int x)
        {
            return f(x);
        }
    }
}
