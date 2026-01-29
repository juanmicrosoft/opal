namespace LogicalOperators
{
    public static class Logic
    {
        public static bool And(bool a, bool b)
        {
            return a && b;
        }

        public static bool Or(bool a, bool b)
        {
            return a || b;
        }

        public static bool Not(bool a)
        {
            return !a;
        }
    }
}
