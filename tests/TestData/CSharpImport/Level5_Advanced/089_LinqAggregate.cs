namespace LinqAggregate
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Aggregations
    {
        public static int GetSum(IEnumerable<int> numbers)
        {
            return numbers.Sum();
        }

        public static double GetAverage(IEnumerable<int> numbers)
        {
            return numbers.Average();
        }

        public static int GetMax(IEnumerable<int> numbers)
        {
            return numbers.Max();
        }

        public static int GetMin(IEnumerable<int> numbers)
        {
            return numbers.Min();
        }

        public static int GetCount(IEnumerable<int> numbers)
        {
            return numbers.Count();
        }

        public static int GetFirst(IEnumerable<int> numbers)
        {
            return numbers.First();
        }

        public static int GetFirstOrDefault(IEnumerable<int> numbers)
        {
            return numbers.FirstOrDefault();
        }
    }
}
