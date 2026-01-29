namespace EnumType
{
    public enum Color
    {
        Red,
        Green,
        Blue
    }

    public static class Colors
    {
        public static Color GetDefault()
        {
            return Color.Red;
        }

        public static bool IsRed(Color c)
        {
            return c == Color.Red;
        }
    }
}
