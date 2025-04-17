using System.CodeDom.Compiler;

namespace DUMPSYM.Extensions;

public static class IndentedTextWriterExtensions
{
    public static IDisposable GetIndentScope(this IndentedTextWriter writer)
    {
        return new IndentScope(writer);
    }

    private readonly struct IndentScope : IDisposable
    {
        private IndentedTextWriter Writer { get; }

        public IndentScope(IndentedTextWriter writer)
        {
            Writer = writer;

            Writer.Indent++;
        }

        public void Dispose()
        {
            Writer.Indent--;
        }
    }
}