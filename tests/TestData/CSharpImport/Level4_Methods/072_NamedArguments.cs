namespace NamedArguments
{
    public static class Geometry
    {
        public static double CalculateArea(double width, double height)
        {
            return width * height;
        }

        public static double CalculateVolume(double width, double height, double depth)
        {
            return width * height * depth;
        }

        public static string FormatDimensions(int x, int y, int z)
        {
            return x.ToString() + "x" + y.ToString() + "x" + z.ToString();
        }
    }

    public static class Demo
    {
        public static double GetArea()
        {
            return Geometry.CalculateArea(width: 10, height: 20);
        }

        public static double GetVolume()
        {
            return Geometry.CalculateVolume(height: 5, width: 3, depth: 2);
        }
    }
}
