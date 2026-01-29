namespace NestedClass
{
    public class Outer
    {
        public int Value;

        public class Inner
        {
            public int InnerValue;

            public int GetValue()
            {
                return InnerValue;
            }
        }

        public Inner CreateInner()
        {
            var inner = new Inner();
            inner.InnerValue = Value * 2;
            return inner;
        }
    }
}
