namespace ArrayDeclaration
{
    public static class Arrays
    {
        public static int[] CreateArray()
        {
            int[] numbers = new int[5];
            numbers[0] = 1;
            numbers[1] = 2;
            numbers[2] = 3;
            numbers[3] = 4;
            numbers[4] = 5;
            return numbers;
        }

        public static int GetFirst(int[] arr)
        {
            return arr[0];
        }

        public static int GetLength(int[] arr)
        {
            return arr.Length;
        }
    }
}
