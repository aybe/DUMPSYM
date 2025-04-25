namespace DUMPSYM.Tests;

public static class Sorting
{
    public static bool TryGetTopologicalSort<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> selector, out List<T> result) where T : notnull
    {
        result = [];

        var array = source as T[] ?? source.ToArray();

        if (array.Length == 0)
        {
            return true;
        }

        var graph = array.ToDictionary(s => s, _ => new List<T>());

        var edges = array.ToDictionary(s => s, _ => 0);

        foreach (var element in array)
        {
            foreach (var dependency in selector(element))
            {
                graph[dependency].Add(element);

                edges[element]++;
            }
        }

        var queue = new Queue<T>(edges.Where(s => s.Value == 0).Select(s => s.Key));

        while (queue.Count > 0)
        {
            var dequeue = queue.Dequeue();

            result.Add(dequeue);

            foreach (var element in graph[dequeue])
            {
                if (--edges[element] == 0)
                {
                    queue.Enqueue(element);
                }
            }
        }

        var b = result.Count == edges.Count;

        result = b ? result : edges.Keys.Except(result).ToList();

        return b;
    }
}