namespace LinqOrderBy
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Sorting
    {
        public static IEnumerable<int> SortAscending(IEnumerable<int> numbers)
        {
            return numbers.OrderBy(x => x);
        }

        public static IEnumerable<int> SortDescending(IEnumerable<int> numbers)
        {
            return numbers.OrderByDescending(x => x);
        }

        public static IEnumerable<Person> SortByName(IEnumerable<Person> people)
        {
            return people.OrderBy(p => p.Name);
        }

        public static IEnumerable<Person> SortByAgeThenName(IEnumerable<Person> people)
        {
            return people.OrderBy(p => p.Age).ThenBy(p => p.Name);
        }
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
