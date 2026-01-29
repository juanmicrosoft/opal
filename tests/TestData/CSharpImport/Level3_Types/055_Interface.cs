namespace InterfaceType
{
    public interface IShape
    {
        double GetArea();
        double GetPerimeter();
    }

    public class Square : IShape
    {
        public double Side;

        public double GetArea()
        {
            return Side * Side;
        }

        public double GetPerimeter()
        {
            return 4 * Side;
        }
    }
}
