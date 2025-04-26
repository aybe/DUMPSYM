using DUMPSYM.Tests.WorkInProgress;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestSdkDetection : UnitTestBase
{
    [TestMethod]
    public void TestDetection()
    {
        foreach (var directory in Directory.GetDirectories(@"C:\Temp\PSX SDKs\extracted"))
        {
            var sdk = PsxSdk.TryIdentify(directory);

            WriteLine(sdk);
        }
    }

    [TestMethod]
    public void TestDistinct()
    {
        var tags = PsxSdk.Versions.SelectMany(s => s.Tags).Select(s => s.Hash).ToArray();

        var duplicates = tags.GroupBy(s => s, StringComparer.OrdinalIgnoreCase).Where(s => s.Count() > 1).Select(g => g.Key).ToArray();

        Assert.AreEqual(0, duplicates.Length, string.Join(", ", duplicates));
    }
}