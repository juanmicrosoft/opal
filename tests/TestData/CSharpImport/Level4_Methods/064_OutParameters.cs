namespace OutParameters
{
    public static class Parser
    {
        public static bool TryParse(string text, out int result)
        {
            if (text == "42")
            {
                result = 42;
                return true;
            }
            result = 0;
            return false;
        }

        public static void GetMinMax(int a, int b, out int min, out int max)
        {
            if (a < b)
            {
                min = a;
                max = b;
            }
            else
            {
                min = b;
                max = a;
            }
        }
    }
}
