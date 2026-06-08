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

        IdaOutputUtility.SplitFiles(sf, sourcePath, targetPath);
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
    public void TestGenerateScripts(string sourcePath, string targetPath)
    {
        var file = SymbolFile.Dump(sourcePath);

        IdaGeneratorUtility.GenerateScripts(file, targetPath);
    }

    #endregion
}