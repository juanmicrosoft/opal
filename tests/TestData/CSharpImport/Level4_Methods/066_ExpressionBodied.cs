namespace ExpressionBodied
{
    public static class Math
    {
        public static int Square(int x) => x * x;

        public static int Cube(int x) => x * x * x;

        public static double Half(double x) => x / 2.0;

        public static bool IsPositive(int x) => x > 0;

        public static int Max(int a, int b) => a > b ? a : b;
    }
}
