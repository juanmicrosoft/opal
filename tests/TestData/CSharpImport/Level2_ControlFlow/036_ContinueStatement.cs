namespace ContinueStatement
{
    public static class Filter
    {
        public static int SumEven(int n)
        {
            int sum = 0;
            for (int i = 1; i <= n; i++)
            {
                if (i % 2 != 0)
                {
                    continue;
                }
                sum = sum + i;
            }
            return sum;
        }
    }
}
