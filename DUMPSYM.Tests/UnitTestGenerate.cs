using System.Runtime.CompilerServices;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestGenerate : UnitTestBase
{
    [TestMethod]
    public void TestTypedefs()
    {
        var list = new LinkedList<SymbolRecord>(UnitTest1.GetSample());

        Console.WriteLine($"Records: {list.Count}");

        var typedefs = list.OfType<SymbolRecordDef>().Where(s => s.Class is SymbolStorageClass.TPDEF).ToArray();

        Console.WriteLine($"Typedefs (raw): {typedefs.Length}");

        list = new LinkedList<SymbolRecord>(list.Except(typedefs));

        typedefs = typedefs.Distinct(SymbolRecordDefEqualityComparer.Instance).ToArray();

        Console.WriteLine($"Typedefs (distinct): {typedefs.Length}");

        Console.WriteLine($"Records: {list.Count}");

        foreach (var def in typedefs)
        {
            Console.WriteLine(GetTypedefString(def));
        }
    }

    private static string GetTypedefString(SymbolRecordDef def)
    {
        Assert.AreEqual(0u, def.Size);

        Assert.AreEqual(SymbolStorageClass.TPDEF, def.Class);

        var handler = new DefaultInterpolatedStringHandler();

        handler.AppendLiteral("typedef ");

        handler.AppendLiteral(GetKindString(def.Type.Kind));

        var modifiers = def.Type.Modifiers.ToArray();

        if (modifiers.Any(s => s == SymbolTypeModifier.ARY))
        {
            throw new NotImplementedException(SymbolTypeModifier.ARY.ToString());
        }

        var fcn = modifiers.Any(s => s == SymbolTypeModifier.FCN);

        if (fcn)
        {
            handler.AppendLiteral(" (");
        }

        if (modifiers.Any(s => s == SymbolTypeModifier.PTR))
        {
            handler.AppendLiteral("*");
        }

        if (!fcn)
        {
            handler.AppendLiteral(" ");
        }

        handler.AppendFormatted(def.Name);

        if (fcn)
        {
            handler.AppendLiteral(")(void)");
        }

        handler.AppendLiteral(";");

        var str = handler.ToStringAndClear();

        return str;
    }

    private static string GetKindString(SymbolTypeKind kind)
    {
        return kind switch
        {
            SymbolTypeKind.NULL   => null,
            SymbolTypeKind.VOID   => "void",
            SymbolTypeKind.CHAR   => "char",
            SymbolTypeKind.SHORT  => "short",
            SymbolTypeKind.INT    => "int",
            SymbolTypeKind.LONG   => "long",
            SymbolTypeKind.FLOAT  => "float",
            SymbolTypeKind.DOUBLE => "double",
            SymbolTypeKind.STRUCT => null,
            SymbolTypeKind.UNION  => null,
            SymbolTypeKind.ENUM   => null,
            SymbolTypeKind.MOE    => null,
            SymbolTypeKind.UCHAR  => "unsigned char",
            SymbolTypeKind.USHORT => "unsigned short",
            SymbolTypeKind.UINT   => "unsigned int",
            SymbolTypeKind.ULONG  => "unsigned long",
            _                     => null
        } ?? throw new NotSupportedException(kind.ToString());
    }
}