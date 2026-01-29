namespace WhileLoop
{
    public static class Loops
    {
        public static int Sum(int n)
        {
            int result = 0;
            int i = 1;
            while (i <= n)
            {
                result = result + i;
                i = i + 1;
            }
            return result;
        }
    }
}
