using DUMPSYM.Symbols;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InvertIf
// ReSharper disable RedundantIfElseBlock

namespace DUMPSYM.Generators;

public sealed class IdaScriptGenerator(IdaGenerator generator)
// TODO encountered classes for parameters: REGPARM, ARG, REG, AUTO
// TODO there are things inside function blocks, see if they're useful
// BUG some functions are wrong, e.g. 8003DBD0 is a stub for 8003D930 and lacks parameters, solution would be to override decls, log warnings
{
    private const string BadMissingReturnType = "BAD_MISSING_RETURN_TYPE";

    private IdaGenerator Generator { get; } = generator;

    private Dictionary<Symbol, Symbol?> Returns { get; set; } = null!;

    private ILookup<uint, Symbol> SymbolsByAddress { get; set; } = null!; // TODO

    private Symbol[] Symbols { get; set; } = null!; // TODO

    public IdaScriptGeneratorOutput Generate(SymbolFile file)
    {
        Symbols = file.Symbols.ToArray();

        SymbolsByAddress = Symbols.ToLookup(s => s.Header.Address);

        var split = Symbol.Split(Symbols);

        var funcs = split.Where(s => s[0].IsFunction).ToArray();

        // BUG: had to use singleOrDefault bc _card_clear @ 8009EF04 in DD has no return type though it should be long 

        Returns = funcs.Select(s => s[0]).ToDictionary(
            s => s,
            s => SymbolsByAddress[s.Header.Address].SingleOrDefault(t => t.Class is SymbolStorageClass.EXT or SymbolStorageClass.STAT));

        var output = new IdaScriptGeneratorOutput();

        foreach (var symbols in funcs)
        {
            var function = ParseFunction(symbols);

            output.Functions.Add(function);
        }

        return output;
    }

    private IdaFunction ParseFunction(Symbol[] symbols)
    {
        var first = symbols[0];

        var function = (ISymbolFunction)first.Record;

        var returns = Returns[first];

        SymbolType type; // TODO ugly hack for DD

        if (returns != null)
        {
            type = returns.Type!.Value;
        }
        else
        {
            type = new SymbolType(SymbolTypeKind.VOID, SymbolTypeModifier.FCN);
        }

        var modifiers = type.Modifiers.ToArray();

        Assert.IsFalse(modifiers.Contains(SymbolTypeModifier.ARY));

        Assert.IsTrue(modifiers.Contains(SymbolTypeModifier.FCN));

        string returnType;

        var pointers = new string('*', modifiers.Count(s => s is SymbolTypeModifier.PTR));

        if (returns?.Tag == null)
        {
            returnType = IdaGenerator.ToString(type.Kind);
        }
        else
        {
            // TODO enable again Assert.IsFalse(returns.HasFakeTag);
            returnType = returns.Tag!;
        }

        var parameters = symbols.TakeWhile(s => s.Record is not ISymbolFunctionBlock).Where(s => s.Record is ISymbolDefinition).ToArray();

        var select = parameters.Where(s => s.Class is SymbolStorageClass.REGPARM or SymbolStorageClass.ARG).Select(GetParameterString).ToArray();

        var text = string.Join(", ", select);

        return new IdaFunction
        {
            Comments = [returns?.ToString() ?? BadMissingReturnType, ..parameters.Select(s => s.ToString())],
            File = function.File,
            Header = first.Header,
            Name = function.Name,
            Declaration = $"{returnType}{pointers} {function.Name}({text});",
        };
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

            return $"{IdaGenerator.ToString(kind)} {type.Name}{pointers} {parameter.Name}";
        }
        else
        {
            Assert.IsTrue(parameter.Tag is null);

            // TODO DRY this typedef override stuff

            var kindEnum = parameter.Type!.Value.Kind;

            var kindText = Generator.TypedefsOverrides.GetValueOrDefault(kindEnum, IdaGenerator.ToString(kindEnum));

            return $"{kindText}{pointers} {parameter.Name}";
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