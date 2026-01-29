namespace AsyncMethod
{
    using System.Threading.Tasks;

    public static class AsyncOperations
    {
        public static async Task<int> GetValueAsync()
        {
            await Task.Delay(100);
            return 42;
        }

        public static async Task<string> GetMessageAsync()
        {
            await Task.Delay(50);
            return "Hello";
        }

        public static async Task DoWorkAsync()
        {
            await Task.Delay(100);
            Console.WriteLine("Work done");
        }
    }
}
