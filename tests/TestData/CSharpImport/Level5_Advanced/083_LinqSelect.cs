namespace LinqSelect
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Queries
    {
        public static IEnumerable<int> DoubleAll(IEnumerable<int> numbers)
        {
            return numbers.Select(x => x * 2);
        }

        public static IEnumerable<string> GetNames(IEnumerable<Person> people)
        {
            return people.Select(p => p.Name);
        }

        public static IEnumerable<int> GetLengths(IEnumerable<string> strings)
        {
            return strings.Select(s => s.Length);
        }
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
