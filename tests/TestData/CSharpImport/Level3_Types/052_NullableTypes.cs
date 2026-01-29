namespace NullableTypes
{
    public static class Nullables
    {
        public static int? GetNullableInt(bool returnNull)
        {
            if (returnNull)
            {
                return null;
            }
            return 42;
        }

        public static bool HasValue(int? value)
        {
            return value.HasValue;
        }

        public static int GetValueOrDefault(int? value, int defaultValue)
        {
            if (value.HasValue)
            {
                return value.Value;
            }
            return defaultValue;
        }
    }
}
