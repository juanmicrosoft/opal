namespace PatternMatching
{
    public static class Patterns
    {
        public static string DescribeObject(object obj)
        {
            if (obj is int i)
            {
                return "Integer: " + i;
            }
            if (obj is string s)
            {
                return "String: " + s;
            }
            if (obj is bool b)
            {
                return "Boolean: " + b;
            }
            return "Unknown";
        }

        public static string DescribeNumber(int n)
        {
            return n switch
            {
                < 0 => "negative",
                0 => "zero",
                > 0 and < 10 => "small positive",
                >= 10 and < 100 => "medium positive",
                _ => "large positive"
            };
        }

        public static int GetLength(object obj)
        {
            return obj switch
            {
                string s => s.Length,
                int[] arr => arr.Length,
                null => 0,
                _ => -1
            };
        }
    }
}
