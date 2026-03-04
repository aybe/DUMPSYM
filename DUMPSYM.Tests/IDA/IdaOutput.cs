using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace DUMPSYM.Tests.IDA;

public sealed record IdaOutput
{
    private IdaOutput(ImmutableArray<IdaOutputFunction> functions)
    {
        Functions = functions;
    }

    public ImmutableArray<IdaOutputFunction> Functions { get; }

    public static IdaOutput Parse(string input)
    {
        var split = Regex.Split(input, @"\r?\n");

        var functions = GetFunctions(split);

        return new IdaOutput([..functions]);
    }

    private static List<IdaOutputFunction> GetFunctions(string[] input)
    {
        var functions = new List<IdaOutputFunction>();

        var ranges = GetRanges(input);

        foreach (var (rf, rc) in ranges)
        {
            var f = rc.Start.Value > rf.Start.Value ? input[rf.Start..rc.Start] : input[rf];

            var c = input[rc];

            var o = new IdaOutputFunction(
                Regex.Match(f[1], @"\b(\w+)\(").Groups[1].Value,
                new IdaOutputChunk(rf, string.Join(Environment.NewLine, f)),
                new IdaOutputChunk(rc, string.Join(Environment.NewLine, c))
            );

            functions.Add(o);
        }

        return functions;
    }

    private static ImmutableSortedSet<int> GetLines(string[] input, Regex regex)
    {
        return input.Index().Where(s => regex.IsMatch(s.Item)).Select(s => s.Index).ToImmutableSortedSet();
    }

    private static IEnumerable<(Range, Range)> GetRanges(string[] input)
    {
        var funcs = GetLines(input, new Regex(@"^//-{5}\s\(([0-9A-F]{8})\)\s-{56}$"));

        var comms = GetLines(input, new Regex(@"^//\s([0-9A-F]{8}):\s(.+)$"));

        var list1 = new List<Range>();

        var list2 = new List<Range>();

        for (var i = 0; i < input.Length; i++)
        {
            if (funcs.Contains(i))
            {
                list1.Add(new Range(i, i + 1));
            }

            if (list1.Count >= 1)
            {
                var span = CollectionsMarshal.AsSpan(list1);

                ref var last = ref span[^1];

                last = new Range(last.Start, i + 1);
            }
        }

        foreach (var range in list1)
        {
            var min = int.MaxValue;
            var max = int.MinValue;

            for (var i = range.Start.Value; i < range.End.Value; i++)
            {
                if (comms.Contains(i))
                {
                    min = Math.Min(min, i);
                    max = Math.Max(max, i);
                }
            }

            list2.Add(min > max ? default : new Range(min, max + 1));
        }

        return list1.Zip(list2);
    }
}