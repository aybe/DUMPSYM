using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace DUMPSYM.Tests;

public abstract class UnitTestBase // TODO move to library
{
    public required TestContext TestContext { get; [UsedImplicitly] set; }

    protected void Write(object? value = null)
    {
        TestContext.Write(value?.ToString());
    }

    protected void WriteLine(object? value = null)
    {
        TestContext.WriteLine(value?.ToString());
    }

    protected void WriteLine<T>(Expression<Func<T>> expression) // T avoids Convert(...) expression
    {
        WriteLine(expression, s => s.ToString());
    }

    protected void WriteLine<T>(Expression<Func<T>> expression, Func<T, object> valueGetter) // T avoids Convert(...) expression
    {
        if (expression.Body is not MemberExpression me)
        {
            throw new ArgumentOutOfRangeException(nameof(expression), expression, null);
        }

        var compile = expression.Compile();

        var value = compile();

        var valueValue = valueGetter(value);

        var message = $"{me.Member.Name}: {valueValue}";

        WriteLine(message);
    }

    protected void WriteLineVar(object? value, [CallerArgumentExpression(nameof(value))] string valueName = null!)
    {
        WriteLine($"{valueName}: {value}");
    }
}