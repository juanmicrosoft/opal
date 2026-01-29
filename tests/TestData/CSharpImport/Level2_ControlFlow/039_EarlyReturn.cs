namespace EarlyReturn
{
    public static class Validator
    {
        public static bool IsValidAge(int age)
        {
            if (age < 0)
            {
                return false;
            }
            if (age > 150)
            {
                return false;
            }
            return true;
        }
    }
}
