using System.Collections.Generic;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions
{
    public static class EnumerableExtensions
    {
        public static string JoinToString<T>(this IEnumerable<T> source, string separator) => string.Join(separator, source);

        public static string JoinToString(this IEnumerable<string> source, string separator) => string.Join(separator, source);
    }
}
