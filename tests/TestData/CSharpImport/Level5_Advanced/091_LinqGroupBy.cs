namespace LinqGroupBy
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Grouping
    {
        public static IEnumerable<IGrouping<int, int>> GroupByModulo(IEnumerable<int> numbers, int divisor)
        {
            return numbers.GroupBy(x => x % divisor);
        }

        public static IEnumerable<IGrouping<int, Person>> GroupByAge(IEnumerable<Person> people)
        {
            return people.GroupBy(p => p.Age);
        }

        public static IEnumerable<IGrouping<char, string>> GroupByFirstLetter(IEnumerable<string> strings)
        {
            return strings.GroupBy(s => s[0]);
        }
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
