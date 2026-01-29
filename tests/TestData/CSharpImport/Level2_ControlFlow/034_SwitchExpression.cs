namespace SwitchExpression
{
    public static class DayOfWeek
    {
        public static string GetDayName(int day)
        {
            return day switch
            {
                1 => "Monday",
                2 => "Tuesday",
                3 => "Wednesday",
                4 => "Thursday",
                5 => "Friday",
                6 => "Saturday",
                7 => "Sunday",
                _ => "Unknown"
            };
        }
    }
}
