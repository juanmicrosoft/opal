namespace LocalFunctions
{
    public static class Calculator
    {
        public static int Calculate(int x, int y)
        {
            int Add(int a, int b)
            {
                return a + b;
            }

            int Multiply(int a, int b)
            {
                return a * b;
            }

            int sum = Add(x, y);
            int product = Multiply(x, y);
            return sum + product;
        }

        public static int Factorial(int n)
        {
            int FactorialInternal(int num)
            {
                if (num <= 1)
                {
                    return 1;
                }
                return num * FactorialInternal(num - 1);
            }

            return FactorialInternal(n);
        }
    }
}
