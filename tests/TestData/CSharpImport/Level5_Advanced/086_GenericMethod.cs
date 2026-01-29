namespace GenericMethod
{
    public static class Utilities
    {
        public static T Identity<T>(T value)
        {
            return value;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        public static T[] CreateArray<T>(T value, int count)
        {
            T[] result = new T[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = value;
            }
            return result;
        }

        public static bool AreEqual<T>(T a, T b) where T : class
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            return a.Equals(b);
        }
    }
}
