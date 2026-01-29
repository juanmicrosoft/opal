namespace StaticVsInstance
{
    public class Counter
    {
        private static int globalCount = 0;
        private int instanceCount = 0;

        public void Increment()
        {
            instanceCount = instanceCount + 1;
            globalCount = globalCount + 1;
        }

        public int GetInstanceCount()
        {
            return instanceCount;
        }

        public static int GetGlobalCount()
        {
            return globalCount;
        }

        public static void ResetGlobal()
        {
            globalCount = 0;
        }
    }
}
