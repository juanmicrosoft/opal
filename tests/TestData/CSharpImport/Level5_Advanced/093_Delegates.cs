namespace Delegates
{
    public delegate int BinaryOperation(int a, int b);
    public delegate bool Predicate(int value);
    public delegate void Logger(string message);

    public static class Operations
    {
        public static int Add(int a, int b)
        {
            return a + b;
        }

        public static int Multiply(int a, int b)
        {
            return a * b;
        }

        public static bool IsPositive(int x)
        {
            return x > 0;
        }

        public static int Apply(BinaryOperation op, int a, int b)
        {
            return op(a, b);
        }

        public static int[] Filter(int[] numbers, Predicate pred)
        {
            int count = 0;
            for (int i = 0; i < numbers.Length; i++)
            {
                if (pred(numbers[i]))
                {
                    count = count + 1;
                }
            }

            int[] result = new int[count];
            int index = 0;
            for (int i = 0; i < numbers.Length; i++)
            {
                if (pred(numbers[i]))
                {
                    result[index] = numbers[i];
                    index = index + 1;
                }
            }
            return result;
        }
    }
}
