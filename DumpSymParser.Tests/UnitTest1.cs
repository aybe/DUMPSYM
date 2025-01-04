using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

// ReSharper disable MemberCanBePrivate.Global

namespace DumpSymParser.Tests;

[TestClass]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public sealed class UnitTest1
{
    [PublicAPI]
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void TestMethod2()
    {
        using var reader = new StreamReader(File.OpenRead(@"C:\GitHub\DumpSymParser\MAIN.TXT"));

        var fail = new List<(int LineIndex, string LineValue)>();

        var lineIndex = 0;

        var symbols = new List<Symbol>();

        var templates = new Symbol[]
        {
            new SymbolSetSldToLineOfFile(),
            new SymbolIncSldLineNumByByte(),
            new SymbolIncSldLineNumByWord(),
            new SymbolIncSldLineNum(),
            new SymbolSetSldToLine(),
            new SymbolEndSldInfo(),

            new SymbolClassDef1(),
            new SymbolClassDef2(),

            new SymbolFunctionStart(),
            new SymbolBlockStart(),
            new SymbolBlockEnd(),
            new SymbolFunctionEnd(),

            new SymbolLabel()
        };

        while (true)
        {
            var line = reader.ReadLine();

            lineIndex++;

            if (line == null)
                break;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            switch (line) // TODO
            {
                case "Header : MND version 1":
                case "Target unit 0":
                    continue;
            }

            var template = templates.FirstOrDefault(s => s.Match(line));

            if (template != null)
            {
                var symbol = template.Parse(line, reader);

                Assert.AreEqual(line, symbol.ToString().Split(Environment.NewLine).First());

                symbols.Add(symbol);

                continue;
            }

            fail.Add((lineIndex, line));
        }

        if (fail.Count > 0)
        {
            Assert.Fail(fail.First().ToString());
        }

        foreach (var symbol in symbols)
        {
            TestContext.WriteLine(symbol.ToString());
        }
    }
}

public abstract partial class Symbol
{
    protected Symbol()
    {
    }

    protected Symbol(string input)
    {
        var match = Regex().Match(input);

        Address = int.Parse(match.Groups[1].Value, NumberStyles.HexNumber);
        Type    = int.Parse(match.Groups[2].Value, NumberStyles.HexNumber);
    }

    public int Address { get; }

    public int Type { get; }

    public override string ToString()
    {
        return $"${Address:x8} {Type:x}";
    }

    public virtual bool Match(string input)
    {
        return Regex().IsMatch(input);
    }

    public abstract Symbol Parse(string input, StreamReader reader);

    [GeneratedRegex(@"^\$([a-z0-9]{8})\s([a-z0-9]+)")]
    private static partial Regex Regex();
}

public sealed partial class SymbolClassDef1 : Symbol
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public SymbolClassDef1()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    private SymbolClassDef1(string input)
        : base(input)
    {
        var match = Regex().Match(input);

        var groups = match.Groups;

        IdClass     = int.Parse(groups[1].Value);
        IdClassType = int.Parse(groups[2].Value);
        Size        = int.Parse(groups[3].Value);
        Name        = groups[4].Value;
    }

    public int IdClass { get; }

    public int IdClassType { get; }

    public int Size { get; }

    public string Name { get; }

    public override string ToString()
    {
        return $"{base.ToString()} Def class {IdClass} type {IdClassType} size {Size} name {Name}";
    }

    public override bool Match(string input)
    {
        return base.Match(input) && Regex().IsMatch(input);
    }

    public override Symbol Parse(string input, StreamReader reader)
    {
        return new SymbolClassDef1(input);
    }

    [GeneratedRegex(@"Def class (\d+) type (\d+) size (\d+) name (.*)$")]
    private static partial Regex Regex();
}

public sealed partial class SymbolClassDef2 : Symbol
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public SymbolClassDef2()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    private SymbolClassDef2(string input)
        : base(input)
    {
        var match = Regex().Match(input);

        var groups = match.Groups;

        IdClass     = int.Parse(groups[1].Value);
        IdClassType = int.Parse(groups[2].Value);
        Size        = int.Parse(groups[3].Value);
        Dims        = groups[4].Value.Trim();
        Tag         = groups[5].Value;
        Name        = groups[6].Value;
    }

    public int IdClass { get; }

    public int IdClassType { get; }

    public int Size { get; }

    public string Dims { get; }

    public string Tag { get; }

    public string Name { get; }

    public override string ToString()
    {
        return $"{base.ToString()} Def2 class {IdClass} type {IdClassType} size {Size} dims {Dims} tag {Tag} name {Name}";
    }

    public override bool Match(string input)
    {
        return base.Match(input) && Regex().IsMatch(input);
    }

    public override Symbol Parse(string input, StreamReader reader)
    {
        return new SymbolClassDef2(input);
    }

    [GeneratedRegex(@"Def2 class (\d+) type (\d+) size (\d+) dims ((?:\d+\s*)+) tag (.*) name (.*)$")]
    private static partial Regex Regex();
}

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public sealed partial class SymbolBlockStart : Symbol
{
    public SymbolBlockStart()
    {
    }

    private SymbolBlockStart(string input)
        : base(input)
    {
        var match = Regex().Match(input);

        var groups = match.Groups;

        Line = int.Parse(groups[1].Value);
    }

    public int Line { get; }

    public override string ToString()
    {
        return $"{base.ToString()} Block start  line = {Line}";
    }

    public override bool Match(string input)
    {
        return base.Match(input) && Regex().IsMatch(input);
    }

    public override Symbol Parse(string input, StreamReader reader)
    {
        return new SymbolBlockStart(input);
    }

    [GeneratedRegex(@"Block start  line = (\d+)$")]
    private static partial Regex Regex();
}

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public sealed partial class SymbolFunctionEnd : Symbol
{
    public SymbolFunctionEnd()
    {
    }

    private SymbolFunctionEnd(string input)
        : base(input)
    {
        var match = Regex().Match(input);

        var groups = match.Groups;

        Line = int.Parse(groups[1].Value);
    }

    public int Line { get; }

    public override string ToString()
    {
        return $"{base.ToString()} Function end   line {Line}";
    }

    public override bool Match(string input)
    {
        return base.Match(input) && Regex().IsMatch(input);
    }

    public override Symbol Parse(string input, StreamReader reader)
    {
        return new SymbolFunctionEnd(input);
    }

    [GeneratedRegex(@"Function end   line (\d+)$")]
    private static partial Regex Regex();
}

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public sealed partial class SymbolBlockEnd : Symbol
{
    public SymbolBlockEnd()
    {
    }

    private SymbolBlockEnd(string input)
        : base(input)
    {
        var match = Regex().Match(input);

        var groups = match.Groups;

        Line = int.Parse(groups[1].Value);
    }

    public int Line { get; }

    public override string ToString()
    {
        return $"{base.ToString()} Block end  line = {Line}";
    }

    public override bool Match(string input)
    {
        return base.Match(input) && Regex().IsMatch(input);
    }

    public override Symbol Parse(string input, StreamReader reader)
    {
        return new SymbolBlockEnd(input);
    }

    [GeneratedRegex(@"Block end  line = (\d+)$")]
    private static partial Regex Regex();
}

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public sealed partial class SymbolSetSldToLine : Symbol
{
    public SymbolSetSldToLine()
    {
    }

    private SymbolSetSldToLine(string input)
        : base(input)
    {
        var match = Regex().Match(input);

        var groups = match.Groups;

        Line = int.Parse(groups[1].Value);
    }

    public int Line { get; }

    public override string ToString()
    {
        return $"{base.ToString()} Set SLD linenum to {Line}";
    }

    public override bool Match(string input)
    {
        return base.Match(input) && Regex().IsMatch(input);
    }

    public override Symbol Parse(string input, StreamReader reader)
    {
        return new SymbolSetSldToLine(input);
    }

    [GeneratedRegex(@"Set SLD linenum to (\d+)$")]
    private static partial Regex Regex();
}

public sealed partial class SymbolSetSldToLineOfFile : Symbol
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public SymbolSetSldToLineOfFile()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    private SymbolSetSldToLineOfFile(string input)
        : base(input)
    {
        var match = Regex().Match(input);

        var groups = match.Groups;

        Line = int.Parse(groups[1].Value);

        File = groups[2].Value;
    }

    public int Line { get; }

    public string File { get; }

    public override string ToString()
    {
        return $"{base.ToString()} Set SLD to line {Line} of file {File}";
    }

    public override bool Match(string input)
    {
        return base.Match(input) && Regex().IsMatch(input);
    }

    public override Symbol Parse(string input, StreamReader reader)
    {
        return new SymbolSetSldToLineOfFile(input);
    }

    [GeneratedRegex(@"Set SLD to line (\d+) of file (.*)$")]
    private static partial Regex Regex();
}

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public sealed partial class SymbolEndSldInfo : Symbol
{
    public SymbolEndSldInfo()
    {
    }

    private SymbolEndSldInfo(string input)
        : base(input)
    {
    }

    public override bool Match(string input)
    {
        return base.Match(input) && Regex().IsMatch(input);
    }

    public override Symbol Parse(string input, StreamReader reader)
    {
        return new SymbolEndSldInfo(input);
    }

    public override string ToString()
    {
        return $"{base.ToString()} End SLD info";
    }

    [GeneratedRegex("End SLD info$")]
    private static partial Regex Regex();
}

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public sealed partial class SymbolFunctionStart : Symbol
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public SymbolFunctionStart()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    private SymbolFunctionStart(string input, StreamReader reader)
        : base(input)
    {
        Fp         = int.Parse(FpRegex().Match(reader.ReadLine()!).Groups[1].Value);
        FSize      = int.Parse(FSizeRegex().Match(reader.ReadLine()!).Groups[1].Value);
        RetReg     = int.Parse(RetRegRegex().Match(reader.ReadLine()!).Groups[1].Value);
        Mask       = int.Parse(MaskRegex().Match(reader.ReadLine()!).Groups[1].Value, NumberStyles.HexNumber);
        MaskOffset = int.Parse(MaskOffsetRegex().Match(reader.ReadLine()!).Groups[1].Value);
        Line       = int.Parse(LineRegex().Match(reader.ReadLine()!).Groups[1].Value);
        File       = FileRegex().Match(reader.ReadLine()!).Groups[1].Value;
        Name       = NameRegex().Match(reader.ReadLine()!).Groups[1].Value;
    }

    public int Fp { get; }

    public int FSize { get; }

    public int RetReg { get; }

    public int Mask { get; }

    public int MaskOffset { get; }

    public int Line { get; }

    public string File { get; }

    public string Name { get; }

    public override bool Match(string input)
    {
        return base.Match(input) && Regex1().IsMatch(input);
    }

    public override Symbol Parse(string input, StreamReader reader)
    {
        return new SymbolFunctionStart(input, reader);
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        builder
            .AppendLine($"{base.ToString()} Function start")
            .AppendLine($"    fp = {Fp}")
            .AppendLine($"    fsize = {FSize}")
            .AppendLine($"    retreg = {RetReg}")
            .AppendLine($"    mask = ${Mask:x8}")
            .AppendLine($"    maskoffs = {MaskOffset}")
            .AppendLine($"    line = {Line}")
            .AppendLine($"    file = {File}")
            .AppendLine($"    name = {Name}");

        var result = builder.ToString().Trim();

        return result;
    }

    [GeneratedRegex("Function start$")]
    private static partial Regex Regex1();

    [GeneratedRegex(@"^\s{4}fp\s=\s(\d+)$")]
    private static partial Regex FpRegex();

    [GeneratedRegex(@"^\s{4}fsize\s=\s(\d+)$")]
    private static partial Regex FSizeRegex();

    [GeneratedRegex(@"^\s{4}retreg\s=\s(\d+)$")]
    private static partial Regex RetRegRegex();

    [GeneratedRegex(@"^\s{4}mask\s=\s\$([a-z0-9]{8})$")]
    private static partial Regex MaskRegex();

    [GeneratedRegex(@"^\s{4}maskoffs\s=\s(-?\d+)$")]
    private static partial Regex MaskOffsetRegex();

    [GeneratedRegex(@"^\s{4}line\s=\s(\d+)$")]
    private static partial Regex LineRegex();

    [GeneratedRegex(@"^\s{4}file\s=\s(.+)$")]
    private static partial Regex FileRegex();

    [GeneratedRegex(@"^\s{4}name\s=\s(.+)$")]
    private static partial Regex NameRegex();
}

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public sealed partial class SymbolIncSldLineNum : Symbol
{
    public SymbolIncSldLineNum()
    {
    }

    private SymbolIncSldLineNum(string input)
        : base(input)
    {
    }

    public override bool Match(string input)
    {
        return base.Match(input) && Regex().IsMatch(input);
    }

    public override Symbol Parse(string input, StreamReader reader)
    {
        return new SymbolIncSldLineNum(input);
    }

    public override string ToString()
    {
        return $"{base.ToString()} Inc SLD linenum";
    }

    [GeneratedRegex("Inc SLD linenum$")]
    private static partial Regex Regex();
}

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public sealed partial class SymbolIncSldLineNumByByte : Symbol
{
    public SymbolIncSldLineNumByByte()
    {
    }

    private SymbolIncSldLineNumByByte(string input)
        : base(input)
    {
        var match = Regex().Match(input);

        Byte = int.Parse(match.Groups[1].Value);
    }

    public int Byte { get; }

    public override bool Match(string input)
    {
        return base.Match(input) && Regex().IsMatch(input);
    }

    public override Symbol Parse(string input, StreamReader reader)
    {
        return new SymbolIncSldLineNumByByte(input);
    }

    public override string ToString()
    {
        return $"{base.ToString()} Inc SLD linenum by byte {Byte}";
    }

    [GeneratedRegex(@"Inc SLD linenum by byte (\d+)$")]
    private static partial Regex Regex();
}

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public sealed partial class SymbolIncSldLineNumByWord : Symbol
{
    public SymbolIncSldLineNumByWord()
    {
    }

    private SymbolIncSldLineNumByWord(string input)
        : base(input)
    {
        var match = Regex().Match(input);

        Word = int.Parse(match.Groups[1].Value);
    }

    public int Word { get; }

    public override bool Match(string input)
    {
        return base.Match(input) && Regex().IsMatch(input);
    }

    public override Symbol Parse(string input, StreamReader reader)
    {
        return new SymbolIncSldLineNumByWord(input);
    }

    public override string ToString()
    {
        return $"{base.ToString()} Inc SLD linenum by word {Word}";
    }

    [GeneratedRegex(@"Inc SLD linenum by word (\d+)$")]
    private static partial Regex Regex();
}

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public sealed partial class SymbolLabel : Symbol
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public SymbolLabel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    private SymbolLabel(string input)
        : base(input)
    {
        var match = Regex().Match(input);

        Name = match.Groups[1].Value;
    }

    public string Name { get; }

    public override bool Match(string input)
    {
        return base.Match(input) && Regex().IsMatch(input);
    }

    public override Symbol Parse(string input, StreamReader reader)
    {
        return new SymbolLabel(input);
    }

    public override string ToString()
    {
        return $"{base.ToString()} {Name}";
    }

    [GeneratedRegex(@"^\$[a-z0-9]{8}\s2\s(.+)$")]
    private static partial Regex Regex();
}