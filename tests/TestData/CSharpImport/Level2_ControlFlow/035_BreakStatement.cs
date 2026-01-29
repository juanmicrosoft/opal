namespace BreakStatement
{
    public static class Search
    {
        public static int FindFirst(int target)
        {
            int result = -1;
            for (int i = 0; i < 100; i++)
            {
                if (i == target)
                {
                    result = i;
                    break;
                }
            }
            return result;
        }
    }
}
