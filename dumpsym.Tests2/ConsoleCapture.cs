using System.Text;

namespace dumpsym.Tests2;

public sealed class ConsoleCapture : TextWriter
{
    public ConsoleCapture()
    {
        Source = Console.Out;
        Target = new StringWriter();

        Console.SetOut(this);
    }

    private TextWriter Source { get; }

    private StringWriter Target { get; }

    public override Encoding Encoding => Encoding.UTF8;

    public bool OutputToSource { get; set; } = true;

    public bool OutputToTarget { get; set; } = true;

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        Console.SetOut(Source);
    }

    public override void Flush()
    {
        base.Flush();

        Source.Flush();

        Target.Flush();
    }

    public override void Write(char value)
    {
        if (OutputToSource)
        {
            Source.Write(value);
        }

        if (OutputToTarget)
        {
            Target.Write(value);
        }
    }

    public override string ToString()
    {
        Flush();

        var s = Target.ToString();

        return s;
    }
}