namespace NestedLoops
{
    public static class Loops
    {
        public static int MultiplicationTable(int size)
        {
            int sum = 0;
            for (int i = 1; i <= size; i++)
            {
                for (int j = 1; j <= size; j++)
                {
                    sum = sum + (i * j);
                }
            }
            return sum;
        }
    }
}
