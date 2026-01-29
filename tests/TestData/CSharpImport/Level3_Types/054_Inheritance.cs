namespace Inheritance
{
    public class Animal
    {
        public string Name;

        public string GetName()
        {
            return Name;
        }
    }

    public class Dog : Animal
    {
        public string Breed;

        public string Bark()
        {
            return "Woof!";
        }
    }

    public class Cat : Animal
    {
        public string Meow()
        {
            return "Meow!";
        }
    }
}
