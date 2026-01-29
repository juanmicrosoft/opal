namespace AsyncAwaitChain
{
    using System.Threading.Tasks;

    public static class AsyncChain
    {
        public static async Task<int> Step1Async()
        {
            await Task.Delay(10);
            return 1;
        }

        public static async Task<int> Step2Async(int input)
        {
            await Task.Delay(10);
            return input + 10;
        }

        public static async Task<int> Step3Async(int input)
        {
            await Task.Delay(10);
            return input * 2;
        }

        public static async Task<int> RunPipelineAsync()
        {
            int result1 = await Step1Async();
            int result2 = await Step2Async(result1);
            int result3 = await Step3Async(result2);
            return result3;
        }

        public static async Task<int[]> RunParallelAsync()
        {
            var task1 = Step1Async();
            var task2 = Step1Async();
            var task3 = Step1Async();

            await Task.WhenAll(task1, task2, task3);

            return new int[] { task1.Result, task2.Result, task3.Result };
        }
    }
}
