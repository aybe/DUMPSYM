using System.Security.Cryptography;
using System.Text;
using DUMPSYM.Generators;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestZ : UnitTestBase
{
    [TestMethod]
    public void TestMethodY()
    {
        var generator = new IdaScriptGenerator();

        var contents = generator.Generate(Sample.Default);

        WriteLine(contents);

        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(contents)));

        Assert.AreEqual("0023f3ddce4faa080116cb2db0c8f1dd68db0beb1f14914bd7f80228eb2ccb26", hash, true);
    }
}