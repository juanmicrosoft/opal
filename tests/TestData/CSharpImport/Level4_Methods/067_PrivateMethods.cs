namespace PrivateMethods
{
    public class Calculator
    {
        private int lastResult;

        public int Add(int a, int b)
        {
            lastResult = AddInternal(a, b);
            return lastResult;
        }

        private int AddInternal(int a, int b)
        {
            return a + b;
        }

        public int GetLastResult()
        {
            return lastResult;
        }

        private void Reset()
        {
            lastResult = 0;
        }
    }
}
