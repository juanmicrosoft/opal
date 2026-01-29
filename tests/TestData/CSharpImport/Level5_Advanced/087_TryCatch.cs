namespace TryCatch
{
    using System;

    public static class SafeOperations
    {
        public static int SafeDivide(int a, int b)
        {
            try
            {
                return a / b;
            }
            catch (DivideByZeroException)
            {
                return 0;
            }
        }

        public static int SafeParse(string text)
        {
            try
            {
                return int.Parse(text);
            }
            catch (FormatException)
            {
                return -1;
            }
            catch (Exception)
            {
                return -2;
            }
        }

        public static string ReadWithFinally(bool throwError)
        {
            string result = "";
            try
            {
                if (throwError)
                {
                    throw new Exception("Error");
                }
                result = "success";
            }
            catch (Exception ex)
            {
                result = "error: " + ex.Message;
            }
            finally
            {
                result = result + " (cleaned up)";
            }
            return result;
        }
    }
}
