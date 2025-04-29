namespace DUMPSYM.Tests;

public static class LinkedListExtensions
{
    public static void Add<T>(this LinkedList<T> list, T value)
    {
        list.AddLast(value);
    }
}