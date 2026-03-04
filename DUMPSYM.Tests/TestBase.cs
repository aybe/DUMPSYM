using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace DUMPSYM.Tests;

[PublicAPI]
public abstract class TestBase
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
        WriteLine(expression, s => s);
    }

    protected void WriteLine<T>(Expression<Func<T>> expression, Func<T, object?> valueGetter) // T avoids Convert(...) expression
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

    public static IEnumerable<object[]> GetTestData()
    {
        var path = Path.Combine(Solution.Directory, "Tests", "test-ida-generators.json");

        var text = File.ReadAllText(path);

        var data = JsonConvert.DeserializeObject<TestItem[]>(text)!;

        foreach (var item in data)
        {
            yield return [item.Source, item.Target];
        }
    }

    public static string GetTestName(MethodInfo methodInfo, object[] data)
    {
        return $"{methodInfo.Name}(\"{Path.GetFileName((string)data[0])}\")";
    }

    private sealed record TestItem(string Source, string Target);
}