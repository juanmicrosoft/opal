namespace ComplexConditions
{
    public static class Validator
    {
        public static bool IsValidInput(int x, int y, int z)
        {
            if (x > 0 && y > 0 && z > 0)
            {
                if ((x + y > z) && (x + z > y) && (y + z > x))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsInRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }
    }
}
