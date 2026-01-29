namespace NullCoalescing
{
    public static class NullHandling
    {
        public static string GetOrDefault(string value, string defaultValue)
        {
            return value ?? defaultValue;
        }

        public static int GetOrZero(int? value)
        {
            return value ?? 0;
        }

        public static string GetSafe(string value)
        {
            return value ?? "";
        }

        public static int? GetChained(int? a, int? b, int? c)
        {
            return a ?? b ?? c;
        }

        public static string GetLength(string value)
        {
            return value?.Length.ToString() ?? "null";
        }

        public static void SetIfNull(ref string value, string newValue)
        {
            value ??= newValue;
        }
    }
}
