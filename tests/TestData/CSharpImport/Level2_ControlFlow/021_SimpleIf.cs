namespace SimpleIf
{
    public static class Conditionals
    {
        public static string Check(int x)
        {
            if (x > 0)
            {
                return "positive";
            }
            return "not positive";
        }
    }
}
