namespace RefParameters
{
    public static class Swapper
    {
        public static void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }

        public static void Increment(ref int value)
        {
            value = value + 1;
        }

        public static void Reset(ref int value)
        {
            value = 0;
        }
    }
}
