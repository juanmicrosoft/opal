namespace PropertyGetSet
{
    public class Temperature
    {
        private double celsius;

        public double Celsius
        {
            get { return celsius; }
            set { celsius = value; }
        }

        public double Fahrenheit
        {
            get { return celsius * 9.0 / 5.0 + 32.0; }
            set { celsius = (value - 32.0) * 5.0 / 9.0; }
        }

        public double Kelvin
        {
            get { return celsius + 273.15; }
        }
    }
}
