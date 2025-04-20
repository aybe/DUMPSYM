namespace DUMPSYM.Extensions;

public static class CombinationExtensions
{
    public static IEnumerable<IEnumerable<T>> KCombinationWithRepetition<T>(this IEnumerable<T> source, int length)
    {
        ArgumentNullException.ThrowIfNull(source);

        ArgumentOutOfRangeException.ThrowIfNegative(length);

        var indices = new int[length];

        var array = source as T[] ?? source.ToArray();

        var pow = (int)Math.Pow(array.Length, length);

        for (var i = 0; i < pow; i++)
        {
            yield return indices.Select(s => array[s]);

            var indexer = length - 1;

            while (indexer >= 0)
            {
                if (indices[indexer] < array.Length - 1)
                {
                    indices[indexer]++;
                    break;
                }

                indices[indexer] = 0;
                indexer--;
            }

            if (indexer < 0)
            {
                yield break;
            }
        }
    }

    public static T[][] KCombinationWithRepetition<T>(this T[] modifiers, int length)
    {
        return modifiers.AsEnumerable().KCombinationWithRepetition(length).Select(s => s.ToArray()).ToArray(); // nice shit
    }
}