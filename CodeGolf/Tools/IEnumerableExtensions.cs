using System.Diagnostics.Contracts;
using System.Text;

internal static class IEnumerableExtensions
{
    [Pure]
    public static string JoinToString<T>(this IEnumerable<T> values, char seperator = ',')
    => new StringBuilder().AppendJoin(seperator, values.ToArray()).ToString();
}