using DUMPSYM.Tests.WorkInProgress;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestHeader : UnitTestBase
{
    [TestMethod]
    [DynamicData(nameof(TestTopologicalSortData))]
    public void TestTopologicalSort(Header header)
    {
        var b = Sorting.TryGetTopologicalSort(header, s => s, out var result);

        Assert.IsTrue(b);

        foreach (var value in result)
        {
            WriteLine(value);
        }
    }

    public static IEnumerable<object[]> TestTopologicalSortData()
    {
        yield return [Headers.TYPES];
        yield return [Headers.KERNEL];
        yield return [Headers.LIBCD];
        yield return [Headers.LIBDS];
        yield return [Headers.LIBGPU];
        yield return [Headers.LIBGTE];
        yield return [Headers.LIBGS];
        yield return [Headers.LIBHMD];
        yield return [Headers.LIBMCRD];
        yield return [Headers.LIBSND];
        yield return [Headers.LIBSPU];
    }
}