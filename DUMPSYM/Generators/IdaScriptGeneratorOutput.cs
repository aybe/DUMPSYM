using System.CodeDom.Compiler;

namespace DUMPSYM.Generators;

public sealed class IdaScriptGeneratorOutput
{
    public List<IdaFunction> Functions { get; } = [];

    public string GetFunctionsAsDebugString(bool comments = true, bool declarations = true, bool spaced = true)
    {
        using var writer = new StringWriter();

        foreach (var function in Functions)
        {
            writer.WriteLine($"// {function.Name}");
            writer.WriteLine($"// {function.File}");
            writer.WriteLine($"// {function.Header}");

            if (comments)
            {
                foreach (var comment in function.Comments)
                {
                    writer.WriteLine($"// {comment}");
                }
            }

            if (declarations)
            {
                writer.WriteLine(function.Declaration);
            }

            if (spaced)
            {
                writer.WriteLine();
            }
        }

        var s = writer.ToString();

        return s;
    }

    public string GetFunctionsAsPythonList()
    {
        using var writer = new IndentedTextWriter(new StringWriter());

        writer.WriteLine($"# {Functions.Count} function prototypes");

        writer.WriteLine("dumpsym_function_prototypes = [");

        writer.Indent++;

        foreach (var function in Functions)
        {
            writer.WriteLine("""(0x{0:X8}, "{1}"),""", function.Header.Address, function.Declaration);
        }

        writer.Indent--;

        writer.WriteLine("]");

        return writer.InnerWriter.ToString()!;
    }
}