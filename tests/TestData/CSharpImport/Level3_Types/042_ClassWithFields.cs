namespace ClassWithFields
{
    public class Rectangle
    {
        public int Width;
        public int Height;

        public int GetArea()
        {
            return Width * Height;
        }

        public int GetPerimeter()
        {
            return 2 * (Width + Height);
        }
    }
}
