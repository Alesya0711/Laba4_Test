using System.Text.RegularExpressions;
namespace Module.Core;

public class JsonPathExtractor : IJsonPathExtractor
{
    public string? ExtractValue(string json, string path)
    { 
        if (string.IsNullOrEmpty(json)) return null;
        if (string.IsNullOrEmpty(path)) return null;

        var keys = path.Replace("$", "").Trim('.').Split('.');
        string currentObj = json;

        foreach (var key in keys)
        {
            // Регулярное выражение ищет "key" : "value" или "key" : число/булево
            var pattern = $"\"{key}\"\\s*:\\s*(\"[^\"\\\\]*(\\\\.[^\"\\\\]*)*\"|[^{{}}\\[\\],]+)";
            var match = Regex.Match(currentObj, pattern);

            if (!match.Success) return null;

            // Берём часть после двоеточия
            var value = match.Value.Split(':', 2)[1].Trim();
            currentObj = value;
        }

        return currentObj.Trim('"');
    }
}