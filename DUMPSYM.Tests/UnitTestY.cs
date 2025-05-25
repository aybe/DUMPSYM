using System.Security.Cryptography;
using System.Text;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestY : UnitTestBase
{
    [TestMethod]
    public void TestMethodY()
    {
        var generator = new HeaderGenerator(Sample.Default);

        var contents = generator.Generate();

        const string path = @"C:\Files\GitHub\DUMPSYM\MAIN.SYM.H";

        File.WriteAllText(path, contents);

        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(contents)));

        Assert.AreEqual("7c728371592fc8f1d0f50471123556ced99732d39b4c2ed59ee2a49e59bea9c0", hash, true);
    }
}