namespace DoWhileLoop
{
    public static class Loops
    {
        public static int CountDown(int n)
        {
            int count = 0;
            do
            {
                count = count + 1;
                n = n - 1;
            } while (n > 0);
            return count;
        }
    }
}
