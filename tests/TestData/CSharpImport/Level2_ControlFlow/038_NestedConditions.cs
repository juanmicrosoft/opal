namespace NestedConditions
{
    public static class Validator
    {
        public static string Validate(int age, bool hasLicense)
        {
            if (age >= 18)
            {
                if (hasLicense)
                {
                    return "can drive";
                }
                else
                {
                    return "needs license";
                }
            }
            else
            {
                return "too young";
            }
        }
    }
}
