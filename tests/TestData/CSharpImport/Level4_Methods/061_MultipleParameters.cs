namespace MultipleParameters
{
    public static class Calculator
    {
        public static int Add3(int a, int b, int c)
        {
            return a + b + c;
        }

        public static int Add4(int a, int b, int c, int d)
        {
            return a + b + c + d;
        }

        public static double Average(double a, double b, double c)
        {
            return (a + b + c) / 3.0;
        }
    }
}
