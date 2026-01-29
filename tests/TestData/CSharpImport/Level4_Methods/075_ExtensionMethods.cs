namespace ExtensionMethods
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string s)
        {
            return s == null || s.Length == 0;
        }

        public static string Reverse(this string s)
        {
            char[] chars = s.ToCharArray();
            System.Array.Reverse(chars);
            return new string(chars);
        }

        public static int WordCount(this string s)
        {
            if (s.IsNullOrEmpty())
            {
                return 0;
            }
            return s.Split(' ').Length;
        }
    }

    public static class IntExtensions
    {
        public static bool IsEven(this int n)
        {
            return n % 2 == 0;
        }

        public static bool IsOdd(this int n)
        {
            return n % 2 != 0;
        }
    }
}
