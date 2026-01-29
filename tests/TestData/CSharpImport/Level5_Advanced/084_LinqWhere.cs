namespace LinqWhere
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Filters
    {
        public static IEnumerable<int> GetPositive(IEnumerable<int> numbers)
        {
            return numbers.Where(x => x > 0);
        }

        public static IEnumerable<int> GetEven(IEnumerable<int> numbers)
        {
            return numbers.Where(x => x % 2 == 0);
        }

        public static IEnumerable<string> GetLong(IEnumerable<string> strings, int minLength)
        {
            return strings.Where(s => s.Length >= minLength);
        }

        public static IEnumerable<Person> GetAdults(IEnumerable<Person> people)
        {
            return people.Where(p => p.Age >= 18);
        }
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
