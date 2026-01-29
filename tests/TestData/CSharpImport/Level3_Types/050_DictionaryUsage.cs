namespace DictionaryUsage
{
    using System.Collections.Generic;

    public static class Dictionaries
    {
        public static Dictionary<string, int> CreateScores()
        {
            var dict = new Dictionary<string, int>();
            dict["Alice"] = 100;
            dict["Bob"] = 85;
            dict["Charlie"] = 92;
            return dict;
        }

        public static int GetScore(Dictionary<string, int> scores, string name)
        {
            return scores[name];
        }

        public static bool HasKey(Dictionary<string, int> scores, string name)
        {
            return scores.ContainsKey(name);
        }
    }
}
