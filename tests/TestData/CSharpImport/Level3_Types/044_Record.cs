namespace RecordType
{
    public record Point(int X, int Y);

    public static class Geometry
    {
        public static Point Origin()
        {
            return new Point(0, 0);
        }

        public static Point Create(int x, int y)
        {
            return new Point(x, y);
        }
    }
}
