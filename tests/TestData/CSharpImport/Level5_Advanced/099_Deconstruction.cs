namespace Deconstruction
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }
    }

    public static class Examples
    {
        public static (int, int) GetCoordinates()
        {
            return (10, 20);
        }

        public static int SumTuple()
        {
            var (x, y) = GetCoordinates();
            return x + y;
        }

        public static int SumPoint(Point p)
        {
            var (x, y) = p;
            return x + y;
        }

        public static void Swap()
        {
            int a = 1;
            int b = 2;
            (a, b) = (b, a);
        }
    }
}
