namespace StringInterpolation
{
    public static class Formatting
    {
        public static string Greet(string name)
        {
            return $"Hello, {name}!";
        }

        public static string FormatPerson(string name, int age)
        {
            return $"{name} is {age} years old";
        }

        public static string FormatExpression(int a, int b)
        {
            return $"{a} + {b} = {a + b}";
        }

        public static string FormatNumber(double value)
        {
            return $"Value: {value:F2}";
        }

        public static string MultiLine(string title, string body)
        {
            return $@"Title: {title}
Body: {body}";
        }
    }
}
