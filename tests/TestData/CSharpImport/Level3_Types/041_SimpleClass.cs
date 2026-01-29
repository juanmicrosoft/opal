namespace SimpleClass
{
    public class Person
    {
        public string Name;
        public int Age;
    }

    public static class Factory
    {
        public static Person Create()
        {
            var p = new Person();
            p.Name = "Alice";
            p.Age = 30;
            return p;
        }
    }
}
