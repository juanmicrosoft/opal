namespace UsingStatement
{
    using System;
    using System.IO;

    public static class FileOperations
    {
        public static string ReadFile(string path)
        {
            using (var reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }

        public static void WriteFile(string path, string content)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.Write(content);
            }
        }

        public static int CountLines(string path)
        {
            int count = 0;
            using (var reader = new StreamReader(path))
            {
                while (reader.ReadLine() != null)
                {
                    count = count + 1;
                }
            }
            return count;
        }
    }

    public class Resource : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
