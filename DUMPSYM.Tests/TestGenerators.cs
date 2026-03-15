using DUMPSYM.Generators;
using DUMPSYM.Symbols;
using JetBrains.Annotations;

// ReSharper disable StringLiteralTypo

namespace DUMPSYM.Tests;

[TestClass]
[UsedImplicitly]
public sealed class TestGenerators : TestBase
{
    #region New region

    [TestMethod]
    [DynamicData(nameof(GetTestData2), DynamicDataDisplayName = nameof(GetTestName), DynamicDataDisplayNameDeclaringType = typeof(TestBase))]
    public void TestGenerateFiles(string sourcePath, string targetPath)
    {
        var sf = GetSymbolFile();

        var text = File.ReadAllText(sourcePath);

        var sources = IdaOutputUtility.SplitFiles(sf, text);

        Directory.CreateDirectory(targetPath);

        foreach (var source in sources)
        {
            File.WriteAllText(Path.Combine(targetPath, source.Path), source.Text);
        }
    }

    #endregion

    #region New region

    [TestMethod]
    [UsedImplicitly]
    [DynamicData(nameof(GetTestData1), DynamicDataDisplayName = nameof(GetTestName), DynamicDataDisplayNameDeclaringType = typeof(TestBase))]
    public void TestGenerateHeader(string sourcePath, string targetPath)
    {
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException(null, sourcePath);
        }

        var file = SymbolFile.Dump(sourcePath);

        var generate = IdaGeneratorUtility.GenerateHeader(file);

        WriteLine(generate);

        Directory.CreateDirectory(targetPath);

        var path = Path.Combine(targetPath, Path.ChangeExtension(Path.GetFileNameWithoutExtension(sourcePath), ".H"));

        File.WriteAllText(path, generate);
    }

    [TestMethod]
    [UsedImplicitly]
    [DynamicData(nameof(GetTestData1), DynamicDataDisplayName = nameof(GetTestName), DynamicDataDisplayNameDeclaringType = typeof(TestBase))]
    public void TestGenerateScript(string sourcePath, string targetPath)
    {
        var file = SymbolFile.Dump(sourcePath);

        IdaGeneratorUtility.GenerateScripts(file, targetPath);
    }

    #endregion
}