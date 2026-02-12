namespace DUMPSYM.Extensions;

public static class IEnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        public IEnumerable<List<T>> ChunkBy(Func<T, bool> predicate)
        {
            List<T>? chunk = null;

            foreach (var item in source)
            {
                if (predicate(item))
                {
                    if (chunk != null)
                    {
                        yield return chunk;
                    }

                    chunk = [];
                }

                chunk ??= [];

                chunk.Add(item);
            }

            if (chunk != null)
            {
                yield return chunk;
            }
        }
    }
}