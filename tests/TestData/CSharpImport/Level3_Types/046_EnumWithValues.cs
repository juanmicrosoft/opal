namespace EnumWithValues
{
    public enum StatusCode
    {
        Ok = 200,
        NotFound = 404,
        ServerError = 500
    }

    public static class Status
    {
        public static int GetCode(StatusCode status)
        {
            return (int)status;
        }

        public static bool IsSuccess(StatusCode status)
        {
            return status == StatusCode.Ok;
        }
    }
}
