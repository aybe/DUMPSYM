using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using DUMPSYM.Extensions;

// ReSharper disable StringLiteralTypo

// ReSharper disable IdentifierTypo

// ReSharper disable GrammarMistakeInComment

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression

// ReSharper disable CommentTypo
// ReSharper disable RedundantIfElseBlock
// BUG spu_tab typedef has no associated structure
namespace DUMPSYM.Tests;

[TestClass]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed partial class UnitTestXYZ456 : UnitTestBase
{
    private readonly ParseSettings Settings = new()
    {
        ParseTypedef1 = true,
        ParseTypedef2 = true,
        ParseUnknowns = false,
        WithFakeNameSuffix = false,
    };

    private static Regex RegexFakeName { get; } = new(@"^\.(\d+fake)", RegexOptions.Compiled);

    private IndentedTextWriter Writer { get; } = GetWriter();

    private HashSet<ISymbolDefinition> Typedefs1 { get; } = [];

    private HashSet<ISymbolDefinition2> Typedefs2 { get; } = [];

    private HashSet<string> Types { get; } = [];

    private Dictionary<ISymbol, string> Definitions { get; } = new();

    [TestMethod]
    public void Test1()
    {
        Writer.WriteLine("// ReSharper disable CppClangTidyClangDiagnosticUnusedMacros");
        Writer.WriteLine("#ifndef PSX_H");
        Writer.WriteLine("#define PSX_H");
        Writer.WriteLine("// ReSharper disable CppInconsistentNaming");
        Writer.WriteLine("// ReSharper disable CommentTypo");
        Writer.WriteLine("// ReSharper disable IdentifierTypo");
        Writer.WriteLine("// ReSharper disable CppClangTidyClangDiagnosticReservedIdentifier");
        Writer.WriteLine("// ReSharper disable CppClangTidyBugproneReservedIdentifier");
        Writer.WriteLine("// ReSharper disable CppRedundantElaboratedTypeSpecifier");
        Writer.WriteLine("// ReSharper disable CppClangTidyClangDiagnosticGnuEmptyStruct");
        Writer.WriteLine("// ReSharper disable CppClangTidyClangDiagnosticStrictPrototypes");
        Writer.WriteLine("// ReSharper disable GrammarMistakeInComment");
        Writer.WriteLine("#pragma warning(push, 4)");

        Parse([.. Sample.Default.Symbols.Select(s => s.Record)]);

        Writer.WriteLine("#pragma warning(pop)");
        Writer.WriteLine();
        Writer.WriteLine("void test(void)");
        Writer.WriteLine("{");
        Writer.WriteLine("    ");
        Writer.WriteLine("}");
        Writer.WriteLine("#endif /* !PSX_H */");


        var value = Writer.InnerWriter.ToString();

        const string current = @"C:\Files\GitHub\! PSX\DUMPSYM\Project1\current.c";

        const string reference = @"C:\Files\GitHub\! PSX\DUMPSYM\Project1\reference.c";

        File.WriteAllText(current, value);

        if (File.Exists(reference))
        {
            Assert.AreEqual(File.ReadAllText(reference), value);
        }
        else
        {
            File.WriteAllText(reference, value);

            File.SetAttributes(reference, File.GetAttributes(reference) | FileAttributes.ReadOnly);
        }

        Writer.Dispose();
    }

    private void Parse(LinkedList<ISymbol> symbols)
    {
        for (var n = symbols.First; n != null; n = n.Next)
        {
            var symbol = n.Value;

            if (Ignore(symbol))
            {
                continue;
            }

            Parse(ref n);

            if (Settings.AddEmptyLines)
            {
                Writer.WriteLine();
            }
        }

        return;

        [SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
        static bool Ignore(ISymbol symbol)
        {
            switch (symbol)
            {
                case ISymbolFileStart:
                case ISymbolLineModifier:
                case ISymbolFileEnd:
                    return true;
                default:
                    return false;
            }
        }
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    private void Parse(ref LinkedListNode<ISymbol> node)
    {
        if (node.Value.IsTypedef1(out var def1))
        {
            ParseTypedef1(def1);
            return;
        }

        if (node.Value.IsTypedef2(out var def2))
        {
            ParseTypedef2(def2, node);
            return;
        }

        if (node.Value.IsType(out var type))
        {
            ParseType(type, ref node);
            return;
        }

        if (Settings.ParseUnknowns)
        {
            throw new NotImplementedException(node.Value.ToString());
        }
    }

    private void ParseType(ISymbolDefinition type, ref LinkedListNode<ISymbol> node)
    {
        if (type.ToString() == "Def class STRTAG type STRUCT size 40 name _GsCOORDINATE")
        {
        }

        if (type.ToString() == "Def class STRTAG type STRUCT size 140 name CameraWindow")
        {
        }

        if (!node.TryFindNode(out var eos, s => s.IsTypeFooter(type)))
        {
            throw new InvalidOperationException(); // TODO need .Find instead
        }

        var typedef = eos.Next!.Value is ISymbolDefinition2 { Class: SymbolStorageClass.TPDEF } d && d.Tag == type.Name && !d.Type.Modifiers.Any() ? d : null;

        var fake = RegexFakeName.IsMatch(type.Name);

        string name;

        if (fake)
        {
            if (typedef != null)
            {
                name = Settings.WithFakeNameSuffix ? $"{typedef.Name}_{type.Name[1..]}" : $"{typedef.Name}";
            }
            else
            {
                name = $"_{type.Name[1..]}"; // compiler-generated, e.g. 00aa83: $00000000 94 Def class STRTAG type STRUCT size 6 name .91fake
            }
        }
        else
        {
            name = type.Name;
        }

        if (!Types.Add(name))
        {
            return;
        }

        using var writer = GetWriter();

        var klass = ToString(type.Class);

        if (typedef == null || Settings.GenerateTypedefStruct is false)
        {
            writer.WriteLine($"{klass} {name} // {node.Value}");
        }
        else
        {
            // BUG struct CliPt BPoints[9] type is incomplete
            // TODO typedef struct EngineCoord should be typedef struct _95fake
            // TODO struct CliPt BPoints[9] should be CliPt BPoints[9]

            //writer.WriteLine($"{ToString(SymbolStorageClass.TPDEF)} {klass} {name} // {node.Value}");

            //writer.WriteLine($"{ToString(SymbolStorageClass.TPDEF)} {klass} _{type.Name[1..]} // {node.Value}");
            writer.WriteLine($"{ToString(SymbolStorageClass.TPDEF)} {klass} // {node.Value}");
        }

        writer.WriteLine("{");

        using (writer.GetIndentScope())
        {
            for (var n = node.Next; n != null && n != eos; n = n.Next)
            {
                var member = n.Value;

                if (member.ToString() == "Def2 class MOS type PTR STRUCT size 40 dims 0 tag _GsCOORDINATE name super")
                {
                }

                var memberType = GetMemberType(n, out var undecorated /* TODO remove */);

                var memberName = ((ISymbolDefinition)member).Name;

                var memberSize = member is ISymbolDefinition2 m
                    ? string.Concat(m.Dimensions.Select(s => $"[{s}]"))
                    : string.Empty;

                if (member is ISymbolDefinition2 { Type.Kind: SymbolTypeKind.STRUCT or SymbolTypeKind.UNION } e)
                {
                    var b = true;

                    for (var p = node; p != null; p = p.Previous)
                    {
                        if (p.Value is not ISymbolDefinition2 { Class: SymbolStorageClass.TPDEF } k)
                        {
                            continue;
                        }

                        if (k.Tag != e.Tag)
                        {
                            continue;
                        }

                        b = false;
                        break;
                    }

                    if (name == undecorated || b)
                    {
                        writer.WriteLine($"{ToString(e.Type.Kind)} {memberType} {memberName}{memberSize}; // {member}");
                    }
                    else
                    {
                        for (var p = n.Previous; p != null && p != n && p != node; p = p.Previous) // R# struct field cannot be used as a type
                        {
                            if (p.Value is not ISymbolDefinition2 l)
                            {
                                continue;
                            }

                            if (l.Name != memberType)
                            {
                                continue;
                            }

                            // BUG causes C2079 'CameraWindow' uses undefined struct 'CameraWindow'
                            // memberType = $"::{memberType}";
                            break;
                        }

                        // if member name is same as another member type name, append '_'
                        // this fixes R# error 'struct field cannot be used as a type'
                        // better than '::' as it makes 'uses undefined struct' in other places
                        for (var c = node.Next; c != null && c != eos; c = c.Next)
                        {
                            if (c == n) // self
                            {
                                continue;
                            }

                            if (c.Value is not ISymbolDefinition2 sd2)
                            {
                                continue;
                            }

                            if (memberName != sd2.Tag)
                            {
                                continue;
                            }

                            memberName = $"{memberName}_";
                            break;
                        }

                        writer.WriteLine($"{memberType} {memberName}{memberSize}; // {member}");
                    }
                }
                else
                {
                    writer.WriteLine($"{memberType} {memberName}{memberSize}; // {member}");
                }
            }
        }

        if (typedef == null || Settings.GenerateTypedefStruct is false)
        {
            writer.WriteLine("};");
        }
        else
        {
            writer.WriteLine($"}} {name};");
        }

        if (Settings.AddMissingTypedefs)
        {
            if (typedef == null)
            {
                if (Settings.AddEmptyLines)
                {
                    writer.WriteLine();
                }

                writer.WriteLine($"{ToString(SymbolStorageClass.TPDEF)} {klass} {name} {name}; // auto-generated (typedef)");
            }
        }

        var format = writer.InnerWriter.ToString();

        Writer.Write(format);

        node = eos;
    }

    private string GetMemberType(LinkedListNode<ISymbol> node, out string undecorated)
    {
        undecorated = null!; // TODO everywhere

        // BUG short m[3][3]; // should be SWORD m[3][3] // Def2 class MOS type ARY ARY SHORT size 18 dims 2 3 3 tag  name m
        // BUG add pointers everywhere
        // BUG ColVect.Pos1 type is "" // Def2 class MOS type STRUCT size 6 dims 0 tag .91fake name Pos1
        var symbol = node.Value as ISymbolDefinition ?? throw new ArgumentOutOfRangeException(nameof(node));

        if (symbol.ToString() == "Def2 class MOU type STRUCT size 2 dims 0 tag .109fake name WR")
        {
            var i = 0;
        }

        Assert.IsTrue(symbol.Class is SymbolStorageClass.MOS or SymbolStorageClass.MOU or SymbolStorageClass.FIELD, symbol.Class.ToString()); // TODO FIELD

        if (Definitions.TryGetValue(symbol, out var value))
        {
            return value;
        }

        var typedefs = node.List!.Where(s => s.IsTypedef()).Cast<ISymbolDefinition>().ToArray();

        var pointers = new string('*', symbol.Type.Modifiers.Count(s => s is SymbolTypeModifier.PTR));

        if (symbol is ISymbolDefinition2 complex)
        {
            string name; // TODO get typedef name instead of ToString

            var tag = complex.Tag;

            if (string.IsNullOrEmpty(tag))
            {
                name = ToString(symbol.Type.Kind);
            }
            else
            {
                if (RegexFakeName.IsMatch(tag))
                {
                    var temp = default(string);

                    for (var n = node; n != null; n = n.Previous)
                    {
                        if (n.Value is not ISymbolDefinition2 { Class: SymbolStorageClass.TPDEF } d)
                        {
                            continue;
                        }

                        if (d.Tag != tag)
                        {
                            continue;
                        }

                        temp = d.Name;
                        break;
                    }

                    if (temp == null) // compiler-generated, e.g. 00ab66: $00000000 96 Def2 class MOS type STRUCT size 6 dims 0 tag .91fake name Pos1
                    {
                        for (var n = node; n != null; n = n.Previous)
                        {
                            if (n.Value is not ISymbolDefinition2 { Class: SymbolStorageClass.EOS } d)
                            {
                                continue;
                            }

                            if (d.Tag != tag)
                            {
                                continue;
                            }

                            temp = d.Tag;
                            Assert.IsTrue(RegexFakeName.IsMatch(temp));
                            temp = $"_{temp[1..]}"; // TODO extract method
                            break;
                        }
                    }

                    name = temp ?? throw new InvalidOperationException();
                }
                else
                {
                    name = tag;
                }
            }

            undecorated = name;
            var text = $"{name}{pointers}";
            return Definitions[symbol] = text;
        }
        else
        {
            var todo = 0; // TODO
        }

        // #1 same by type // BUG 'Def class MOS type PTR PTR VOID size 0 name Start' = SpuIRQCallbackProc

        var def1 = typedefs.FirstOrDefault(s => s.Type == symbol.Type);

        if (def1 != null)
        {
            return Definitions[symbol] = def1.Name;
        }

        // #2 same by kind

        var def2 = typedefs.FirstOrDefault(s => s.Type.Kind == symbol.Type.Kind && !s.Type.Modifiers.Any());

        if (def2 != null)
        {
            return Definitions[symbol] = def2.Name;
        }

        // #3 doesn't exist, e.g. no INT in .SYM file -> Def2 class MOS type ARY INT size 4 dims 1 1 tag  name r

        var def3 = $"{ToString(symbol.Type.Kind)}{pointers}";

        return Definitions[symbol] = def3;
    }

    private void ParseTypedef1(ISymbolDefinition def)
    {
        if (!Settings.ParseTypedef1)
        {
            return;
        }

        if (!Typedefs1.Add(def))
        {
            return;
        }

        string value;

        var kind = ToString(def.Type.Kind);

        var name = def.Name;

        var mods = def.Type.Modifiers.ToArray();

        if (mods.Any(s => s is SymbolTypeModifier.FCN))
        {
            Assert.AreEqual(1, mods.Count(s => s is SymbolTypeModifier.FCN));

            value = $"typedef {kind} (*{name})(void); // {def}";
        }
        else
        {
            var pointers = new string('*', mods.Count(s => s is SymbolTypeModifier.PTR));

            value = $"typedef {kind}{pointers} {name}; // {def}";
        }

        Writer.WriteLine(value);
    }

    private void ParseTypedef2(ISymbolDefinition2 def, LinkedListNode<ISymbol> node)
    {
        if (def.ToString() == "Def2 class TPDEF type STRUCT size 3 dims 0 tag .109fake name Palette")
        {
        }

        if (def.ToString() == "Def2 class TPDEF type STRUCT size 16 dims 0 tag .95fake name CliPt")
        {
        }

        if (!Settings.ParseTypedef2)
        {
            return;
        }

        if (!Typedefs2.Add(def))
        {
            return;
        }

        if (Settings.GenerateTypedefStruct)
        {
            if (def.Name == def.Name)
            {
                if (node.Previous!.Value is ISymbolDefinition2 { Class: SymbolStorageClass.EOS } d)
                {
                    if (d.Tag == def.Tag)
                    {
                        return; // don't generate 'typedef', struct will be 'typedef struct' instead
                    }
                }
            }
        }

        var mods = def.Type.Modifiers.ToArray();

        Assert.AreEqual(0, mods.Count(s => s is SymbolTypeModifier.FCN));

        var kind = ToString(def.Type.Kind);

        var tag = Settings.WithFakeNameSuffix ? RegexFakeName.Replace(def.Tag, $"{def.Name}_$1") : def.Name;

        var pointers = new string('*', mods.Count(s => s is SymbolTypeModifier.PTR));

        var fake = RegexFakeName.IsMatch(def.Tag);

        var value = $"{ToString(def.Class)} {kind} {(fake ? tag : def.Tag)}{pointers} {def.Name}; // {def}";


        // TODO search upstream for a typedef with same tag, if any, use its name instead of def.tag

        string newTag = null;

        for (var n = node.Previous; n != null; n = n.Previous)
        {
            if (n.Value is ISymbolDefinition2 d)
            {
                if (d.Class is SymbolStorageClass.TPDEF)
                {
                    if (d.Type.Kind is SymbolTypeKind.STRUCT)
                    {
                        if (d.Tag == def.Tag)
                        {
                            newTag = d.Name;
                            break;
                        }
                    }
                }
            }
        }

        if (newTag != null)
        {
            value = $"{ToString(def.Class)} {newTag}{pointers} {def.Name}; // {def}";
        }
        else
        {
            value = $"{ToString(def.Class)} {kind} {(fake ? $"_{def.Tag[1..]}" : def.Tag)}{pointers} {def.Name}; // {def}";
        }

        Writer.WriteLine(value);
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed partial class UnitTestXYZ456
{
    private static string ToString(SymbolStorageClass value)
    {
        return value switch // TODO reuse for typedef and others
        {
            SymbolStorageClass.STRTAG => "struct",
            SymbolStorageClass.UNTAG  => "union",
            SymbolStorageClass.TPDEF  => "typedef",
            _                         => throw new ArgumentOutOfRangeException(value.ToString()),
        };
    }

    private static string ToString(SymbolTypeKind value)
    {
        return value switch
        {
            SymbolTypeKind.NULL   => "null",
            SymbolTypeKind.VOID   => "void",
            SymbolTypeKind.CHAR   => "char",
            SymbolTypeKind.SHORT  => "short",
            SymbolTypeKind.INT    => "int",
            SymbolTypeKind.LONG   => "long",
            SymbolTypeKind.FLOAT  => "float",
            SymbolTypeKind.DOUBLE => "double",
            SymbolTypeKind.STRUCT => "struct",
            SymbolTypeKind.UNION  => "union",
            SymbolTypeKind.ENUM   => "enum",
            SymbolTypeKind.MOE    => "enum member",
            SymbolTypeKind.UCHAR  => "unsigned char",
            SymbolTypeKind.USHORT => "unsigned short",
            SymbolTypeKind.UINT   => "unsigned int",
            SymbolTypeKind.ULONG  => "unsigned long",
            _                     => throw new ArgumentOutOfRangeException(value.ToString()),
        };
    }

    private static IndentedTextWriter GetWriter()
    {
        return new IndentedTextWriter(new StringWriter());
    }

    [SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer")]
    private record ParseSettings
    {
        public bool ParseTypedef1 { get; init; } = true;

        public bool ParseTypedef2 { get; init; } = true;

        public bool ParseUnknowns { get; init; } = false;

        [Obsolete("Either remove or do typedef struct...")] // TODO
        public bool AddMissingTypedefs { get; } = false;

        public bool WithFakeNameSuffix { get; init; } = false;

        public bool AddEmptyLines { get; } = false; // BUG adds too many atm

        public bool GenerateTypedefStruct { get; } = true;
    }
}