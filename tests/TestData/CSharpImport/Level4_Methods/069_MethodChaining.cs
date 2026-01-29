namespace MethodChaining
{
    public class StringBuilder
    {
        private string value = "";

        public StringBuilder Append(string text)
        {
            value = value + text;
            return this;
        }

        public StringBuilder AppendLine(string text)
        {
            value = value + text + "\n";
            return this;
        }

        public StringBuilder Clear()
        {
            value = "";
            return this;
        }

        public string Build()
        {
            return value;
        }
    }
}
