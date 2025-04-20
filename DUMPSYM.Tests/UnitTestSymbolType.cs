using DUMPSYM.Extensions;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestSymbolType : UnitTestBase
{
    [TestMethod]
    public void SymbolTypeCombinations()
    {
        var kinds = Enum.GetValues<SymbolTypeKind>();

        var modifiers = Enum.GetValues<SymbolTypeModifier>();

        var index = 0;

        var set = new SortedSet<ushort>();

        using var writer = new StringWriter();

        for (var i = 0; i <= 6; i++)
        {
            var repetition = modifiers.KCombinationWithRepetition(i);

            foreach (var kind in kinds)
            {
                foreach (var array in repetition)
                {
                    var type = new SymbolType(kind, array);
                    var x = $"{string.Join(" ", array)} {kind}".Trim();
                    var y = $"{type}".Trim();
                    writer.WriteLine($"{index++,5} | {x} | {y} | {type.Value}");

                    Assert.AreEqual(x, y);

                    set.Add(type.Value);
                }
            }
        }

        WriteLineVar(set.Count);
        WriteLineVar(set.Min);
        WriteLineVar(set.Max);

        WriteLine(writer.ToString());
    }
}