namespace MethodOverloads
{
    public static class Calculator
    {
        public static int Add(int a, int b)
        {
            return a + b;
        }

        public static double Add(double a, double b)
        {
            return a + b;
        }

        public static int Add(int a, int b, int c)
        {
            return a + b + c;
        }

        public static string Add(string a, string b)
        {
            return a + b;
        }
    }
}
