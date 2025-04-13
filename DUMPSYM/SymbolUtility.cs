using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DUMPSYM;

public static class SymbolUtility
{
    private static JsonSerializerSettings Settings { get; } = new()
    {
        ContractResolver = new SymbolFileContractResolver(),
        TypeNameHandling = TypeNameHandling.Auto
    };

    public static SymbolFile DeserializeFile(string json)
    {
        var file = JsonConvert.DeserializeObject<SymbolFile>(json, Settings) ?? throw new ArgumentOutOfRangeException(nameof(json));

        return file;
    }

    public static List<Symbol> DeserializeList(string json)
    {
        var list = JsonConvert.DeserializeObject<List<Symbol>>(json, Settings) ?? throw new ArgumentOutOfRangeException(nameof(json));

        return list;
    }

    public static string SerializeFile(SymbolFile file, Formatting formatting = Formatting.None)
    {
        var json = JsonConvert.SerializeObject(file, formatting, Settings);

        return json;
    }

    public static string SerializeList(List<Symbol> symbols, Formatting formatting = Formatting.None)
    {
        var json = JsonConvert.SerializeObject(symbols, formatting, Settings);

        return json;
    }

    private sealed class SymbolFileContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            if (typeof(SymbolFile).IsAssignableFrom(objectType))
            {
                return CreateObjectContract(objectType);
            }

            return base.CreateContract(objectType);
        }
    }
}