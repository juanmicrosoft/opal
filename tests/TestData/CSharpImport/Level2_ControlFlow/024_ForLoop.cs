namespace ForLoop
{
    public static class Loops
    {
        public static int Sum(int n)
        {
            int result = 0;
            for (int i = 1; i <= n; i++)
            {
                result = result + i;
            }
            return result;
        }
    }
}
