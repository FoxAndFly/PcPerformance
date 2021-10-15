using System.Collections.Generic;

namespace PcPerformance.Helper
{
    public static class DictionaryExtensions
    {
        public static TValue? GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue? defaultValue = default)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}
