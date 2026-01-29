namespace VoidMethods
{
    public static class Logger
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        public static void LogError(string message)
        {
            Console.WriteLine("ERROR: " + message);
        }

        public static void LogWarning(string message)
        {
            Console.WriteLine("WARNING: " + message);
        }

        public static void LogInfo(string message)
        {
            Console.WriteLine("INFO: " + message);
        }
    }
}
