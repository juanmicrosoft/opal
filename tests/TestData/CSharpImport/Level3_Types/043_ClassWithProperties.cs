namespace ClassWithProperties
{
    public class Circle
    {
        public double Radius { get; set; }

        public double GetArea()
        {
            return 3.14159 * Radius * Radius;
        }

        public double GetCircumference()
        {
            return 2 * 3.14159 * Radius;
        }
    }
}
