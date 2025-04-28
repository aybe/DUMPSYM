using System.Diagnostics.CodeAnalysis;

namespace DUMPSYM.Tests;

[TestClass]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class UnitTestXYZ : UnitTestBase
{
    [TestMethod]
    public void TestNamesNotSortedByAddress()
    {
        var check = Check(Sample.Default.Records.Where(s => s.Value is ISymbolVariable).Select(s => s.Key));

        Assert.IsFalse(check); // expected, it's garbage

        return;

        static bool Check(IEnumerable<SymbolHeader> headers)
        {
            var last = -1L;

            foreach (var header in headers)
            {
                var address = header.Address;

                if (address <= last)
                {
                    return false;
                }

                last = address;
            }

            return true;
        }
    }
}