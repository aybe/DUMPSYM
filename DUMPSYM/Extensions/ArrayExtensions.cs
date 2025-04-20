namespace DUMPSYM.Extensions;

public static class ArrayExtensions
{
    public static bool TryGetRange<T>(this T[] array, int start, Predicate<T> header, Predicate<T> footer, out Range range)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);

        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(start, array.Length);

        range = default;

        if (!header(array[start]))
        {
            return false;
        }

        var index = Array.FindIndex(array, start, footer);

        if (index == -1)
        {
            return false;
        }

        range = start..(index + 1);

        return true;
    }
}