using Newtonsoft.Json;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestSerialize : UnitTestBase
{
    [TestMethod]
    public void TestSerializeFile()
    {
        var file = Sample.Default;

        TestSerialization(o => SymbolUtility.SerializeFile((SymbolFile)o, Formatting.Indented), SymbolUtility.DeserializeFile, file);
    }

    [TestMethod]
    public void TestSerializeSymbols()
    {
        var file = Sample.Default;

        TestSerialization(s => SymbolUtility.SerializeList((List<Symbol>)s, Formatting.Indented), SymbolUtility.DeserializeList, file.Symbols);
    }

    private void TestSerialization(Func<object, string> serialize, Func<string, object> deserialize, object value)
    {
        var json1 = serialize(value);

        PrintLines(json1);

        var output1 = deserialize(json1);

        var json2 = serialize(output1);

        using var reader1 = new StringReader(json1);
        using var reader2 = new StringReader(json2);

        var index = 0;

        while (true)
        {
            var line1 = reader1.ReadLine();
            var line2 = reader2.ReadLine();

            if (line1 == null && line2 == null)
            {
                break;
            }

            if (line1 != line2)
            {
                Assert.AreEqual(line1, line2, $"@ line {index}");
            }

            index++;
        }
    }

    private void PrintLines(string text)
    {
        using var reader = new StringReader(text);

        var index = 0;

        while (true)
        {
            var line = reader.ReadLine();

            if (line == null)
            {
                break;
            }

            WriteLine($"{index,8} {line}");

            index++;
        }
    }
}