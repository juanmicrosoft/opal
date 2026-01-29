namespace AbstractClass
{
    public abstract class Shape
    {
        public abstract double GetArea();

        public string Describe()
        {
            return "This is a shape";
        }
    }

    public class Triangle : Shape
    {
        public double Base;
        public double Height;

        public override double GetArea()
        {
            return 0.5 * Base * Height;
        }
    }
}
