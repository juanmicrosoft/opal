namespace TupleType
{
    public static class Tuples
    {
        public static (int, int) GetMinMax(int a, int b)
        {
            if (a < b)
            {
                return (a, b);
            }
            return (b, a);
        }

        public static (string Name, int Age) GetPerson()
        {
            return ("Alice", 30);
        }

        public static int GetFirst((int, int) tuple)
        {
            return tuple.Item1;
        }
    }
}
