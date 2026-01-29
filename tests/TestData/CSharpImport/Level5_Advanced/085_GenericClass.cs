namespace GenericClass
{
    public class Box<T>
    {
        private T value;

        public Box(T initial)
        {
            value = initial;
        }

        public T GetValue()
        {
            return value;
        }

        public void SetValue(T newValue)
        {
            value = newValue;
        }

        public bool HasValue()
        {
            return value != null;
        }
    }

    public class Pair<T1, T2>
    {
        public T1 First { get; set; }
        public T2 Second { get; set; }

        public Pair(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }
    }
}
