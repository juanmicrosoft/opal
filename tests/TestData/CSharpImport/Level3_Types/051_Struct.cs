namespace StructType
{
    public struct Vector2
    {
        public double X;
        public double Y;

        public double GetMagnitude()
        {
            return System.Math.Sqrt(X * X + Y * Y);
        }
    }

    public static class Vectors
    {
        public static Vector2 Create(double x, double y)
        {
            var v = new Vector2();
            v.X = x;
            v.Y = y;
            return v;
        }
    }
}
