namespace ListUsage
{
    using System.Collections.Generic;

    public static class Lists
    {
        public static List<int> CreateList()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);
            return list;
        }

        public static int GetCount(List<int> list)
        {
            return list.Count;
        }

        public static int GetFirst(List<int> list)
        {
            return list[0];
        }
    }
}
