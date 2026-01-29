namespace OptionalParameters
{
    public static class Greeter
    {
        public static string Greet(string name, string greeting = "Hello")
        {
            return greeting + ", " + name + "!";
        }

        public static int Add(int a, int b = 0, int c = 0)
        {
            return a + b + c;
        }

        public static string Format(string text, bool uppercase = false, bool trim = true)
        {
            if (trim)
            {
                text = text.Trim();
            }
            if (uppercase)
            {
                text = text.ToUpper();
            }
            return text;
        }
    }
}
