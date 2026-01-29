namespace IndexerProperty
{
    public class IntArray
    {
        private int[] data;
        private int size;

        public IntArray(int capacity)
        {
            data = new int[capacity];
            size = 0;
        }

        public int this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public int Count
        {
            get { return size; }
        }

        public void Add(int value)
        {
            data[size] = value;
            size = size + 1;
        }
    }
}
