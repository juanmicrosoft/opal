namespace ParamsArray
{
    public static class Calculator
    {
        public static int Sum(params int[] numbers)
        {
            int total = 0;
            for (int i = 0; i < numbers.Length; i++)
            {
                total = total + numbers[i];
            }
            return total;
        }

        public static string Join(string separator, params string[] items)
        {
            if (items.Length == 0)
            {
                return "";
            }
            string result = items[0];
            for (int i = 1; i < items.Length; i++)
            {
                result = result + separator + items[i];
            }
            return result;
        }
    }
}
