namespace ImplicitExplicitConversion
{
    public struct Celsius
    {
        public double Temperature;

        public Celsius(double temp)
        {
            Temperature = temp;
        }

        public static implicit operator double(Celsius c)
        {
            return c.Temperature;
        }

        public static explicit operator Celsius(double d)
        {
            return new Celsius(d);
        }
    }

    public struct Fahrenheit
    {
        public double Temperature;

        public Fahrenheit(double temp)
        {
            Temperature = temp;
        }

        public static implicit operator Fahrenheit(Celsius c)
        {
            return new Fahrenheit(c.Temperature * 9.0 / 5.0 + 32.0);
        }

        public static explicit operator Celsius(Fahrenheit f)
        {
            return new Celsius((f.Temperature - 32.0) * 5.0 / 9.0);
        }
    }
}
