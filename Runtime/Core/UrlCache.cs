using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cache.Core
{
    public class UrlCache : ObjectCache
    {
        private readonly List<Regex> rules = new List<Regex>();

        public UrlCache(int size, TimeSpan? ttl = null) : base(size, ttl) {}

        public void AddRule(Regex rx)
        {
            if (rules.Contains(rx))
            {
                UnityEngine.Debug.LogError($"Could not add url cache rule: duplicate rule {rx}");
                return;
            }

            rules.Add(rx);
        }

        public bool MatchRules(string str)
        {
            foreach (var rule in rules)
            {
                if (rule.IsMatch(str)) return true;
            }

            return false;
        }
    }
}