using System.CodeDom.Compiler;
using DUMPSYM.Generators;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InvertIf
// ReSharper disable RedundantIfElseBlock

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestZ : UnitTestBase
// TODO return nice typedef like u_long
// TODO encountered classes for parameters: REGPARM, ARG, REG, AUTO
// TODO there are things inside function blocks, see if they're useful
{
    private Dictionary<Symbol, Symbol> Returns { get; set; } = null!;

    private ILookup<uint, Symbol> SymbolsByAddress { get; set; } = null!; // TODO

    private Symbol[] Symbols { get; set; } = null!; // TODO

    private bool WriteFunctionName { get; } = true;

    private bool WriterFunctionReturn { get; } = true;

    private bool WriteFunctionParameters { get; } = true;

    private bool WriteFunctionSignature { get; } = true;

    private bool WriteNewLines { get; } = true;

    private IndentedTextWriter Writer { get; } = new(new StringWriter());

    [TestCleanup]
    public void TestCleanup()
    {
        Writer.Dispose();
    }

    [TestMethod]
    public void TestMethodY()
    {
        Symbols = Sample.Default.Symbols.ToArray();

        SymbolsByAddress = Symbols.ToLookup(s => s.Header.Address);

        var split = Symbol.Split(Symbols);

        var funcs = split.Where(s => s[0].IsFunction).ToArray();

        Returns = funcs.Select(s => s[0]).ToDictionary(
            s => s,
            s => SymbolsByAddress[s.Header.Address].Single(t => t.Class is SymbolStorageClass.EXT or SymbolStorageClass.STAT));

        foreach (var symbols in funcs)
        {
            ParseFunction(symbols);
        }

        var value = Writer.InnerWriter.ToString()!;

        WriteLine(value);
    }

    private void ParseFunction(Symbol[] symbols)
    {
        var first = symbols[0];

        var function = (ISymbolFunction)first.Record;

        if (WriteFunctionName)
        {
            Writer.WriteLine($"// {first.Header} {function.Name} {function.File}");
        }

        var returns = Returns[first];

        if (WriterFunctionReturn)
        {
            Writer.WriteLine($"// {returns}");
        }

        var type = returns.Type!.Value;

        var modifiers = type.Modifiers.ToArray();

        Assert.IsFalse(modifiers.Contains(SymbolTypeModifier.ARY));

        Assert.IsTrue(modifiers.Contains(SymbolTypeModifier.FCN));

        string returnType;

        var pointers = new string('*', modifiers.Count(s => s is SymbolTypeModifier.PTR));

        if (returns.Tag == null)
        {
            returnType = SymbolGenerator.ToString(type.Kind);
        }
        else
        {
            Assert.IsFalse(returns.HasFakeTag);
            returnType = returns.Name!;
        }

        var parameters = symbols.TakeWhile(s => s.Record is not ISymbolFunctionBlock).Where(s => s.Record is ISymbolDefinition).ToArray();

        var select = parameters.Where(s => s.Class is SymbolStorageClass.REGPARM or SymbolStorageClass.ARG).Select(GetParameterString).ToArray();

        var text = string.Join(", ", select);

        if (WriteFunctionParameters)
        {
            foreach (var parameter in parameters)
            {
                Writer.WriteLine($"// {parameter}");
            }
        }

        if (WriteFunctionSignature)
        {
            Writer.WriteLine($"{returnType}{pointers} {function.Name}({text});");
        }

        if (WriteNewLines)
        {
            Writer.WriteLine();
        }
    }

    private string GetParameterString(Symbol parameter)
    {
        var modifiers = parameter.Type!.Value.Modifiers.ToArray();

        if (modifiers.Any(s => s is SymbolTypeModifier.ARY))
        {
            throw new NotSupportedException();
        }

        if (modifiers.Any(s => s is SymbolTypeModifier.FCN))
        {
            throw new NotSupportedException();
        }

        var pointers = new string('*', modifiers.Count(s => s is SymbolTypeModifier.PTR));

        var type = GetParameterType(parameter);

        if (type != null)
        {
            Assert.IsTrue(parameter.Tag is not null);

            if (type.Tag == null)
            {
                Assert.IsTrue(type.Class is SymbolStorageClass.STRTAG or SymbolStorageClass.UNTAG, type.ToString());
            }
            else // when tag is fake, name is the type
            {
                Assert.IsTrue(type.Class is SymbolStorageClass.TPDEF, type.ToString());

                Assert.IsFalse(type.HasFakeName);
            }

            var kind = type.Type!.Value.Kind;

            Assert.IsTrue(kind is SymbolTypeKind.STRUCT or SymbolTypeKind.UNION);

            return $"{SymbolGenerator.ToString(kind)} {type.Name}{pointers} {parameter.Name}";
        }
        else
        {
            Assert.IsTrue(parameter.Tag is null);

            return $"{SymbolGenerator.ToString(parameter.Type!.Value.Kind)}{pointers} {parameter.Name}";
        }
    }

    private Symbol? GetParameterType(Symbol symbol)
    {
        if (symbol.Tag == null)
        {
            return null;
        }

        var index1 = Array.IndexOf(Symbols, symbol);

        if (index1 == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(symbol), symbol, null);
        }

        var index2 = Array.FindLastIndex(Symbols, index1, s => s.IsTypeDefinition && s.Tag == symbol.Tag && !s.Type!.Value.Modifiers.Any());

        if (index2 != -1)
        {
            return Symbols[index2];
        }

        var index3 = Array.FindLastIndex(Symbols, index1, s => s.IsTypeHeader && s.Name == symbol.Tag);

        if (index3 != -1)
        {
            return Symbols[index3];
        }

        throw new InvalidOperationException(symbol.ToString());
    }
}