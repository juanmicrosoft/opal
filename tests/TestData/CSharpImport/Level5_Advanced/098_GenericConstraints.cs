namespace GenericConstraints
{
    using System;

    public interface IIdentifiable
    {
        int Id { get; }
    }

    public class Repository<T> where T : class, IIdentifiable, new()
    {
        private T[] items = new T[100];
        private int count = 0;

        public void Add(T item)
        {
            items[count] = item;
            count = count + 1;
        }

        public T FindById(int id)
        {
            for (int i = 0; i < count; i++)
            {
                if (items[i].Id == id)
                {
                    return items[i];
                }
            }
            return null;
        }

        public T CreateNew()
        {
            return new T();
        }
    }

    public class ComparableBox<T> where T : IComparable<T>
    {
        public T Value { get; set; }

        public int CompareTo(ComparableBox<T> other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool IsGreaterThan(ComparableBox<T> other)
        {
            return Value.CompareTo(other.Value) > 0;
        }
    }
}
