namespace VirtualMethods
{
    public class Animal
    {
        public virtual string Speak()
        {
            return "...";
        }

        public virtual string GetType()
        {
            return "Animal";
        }
    }

    public class Dog : Animal
    {
        public override string Speak()
        {
            return "Woof!";
        }

        public override string GetType()
        {
            return "Dog";
        }
    }

    public class Cat : Animal
    {
        public override string Speak()
        {
            return "Meow!";
        }

        public override string GetType()
        {
            return "Cat";
        }
    }
}
