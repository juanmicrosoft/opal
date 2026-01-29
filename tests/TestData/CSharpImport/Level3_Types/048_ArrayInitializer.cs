namespace ArrayInitializer
{
    public static class Arrays
    {
        public static int[] CreateNumbers()
        {
            return new int[] { 1, 2, 3, 4, 5 };
        }

        public static string[] CreateNames()
        {
            return new string[] { "Alice", "Bob", "Charlie" };
        }

        public static int Sum(int[] numbers)
        {
            int total = 0;
            for (int i = 0; i < numbers.Length; i++)
            {
                total = total + numbers[i];
            }
            return total;
        }
    }
}
