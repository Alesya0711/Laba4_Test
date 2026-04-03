namespace Module.Core;

public class JsonPathExtractor : IJsonPathExtractor
{
    public string? ExtractValue(string json, string path)
    {
        if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(path)) return null;

        var keys = path.TrimStart('$', '.').Split('.');
        string currentJson = json;

        foreach (var key in keys)
        {
            currentJson = FindValueByKey(currentJson, key);
            if (currentJson == null) return null;
        }

        return CleanValue(currentJson);
    }

    private string? FindValueByKey(string json, string key)
    {
        var searchKey = $"\"{key}\"";
        var keyIndex = json.IndexOf(searchKey);
        if (keyIndex == -1) return null;

        var colonIndex = json.IndexOf(':', keyIndex);
        if (colonIndex == -1) return null;

        var startValue = colonIndex + 1;
        while (startValue < json.Length && char.IsWhiteSpace(json[startValue]))
            startValue++;

        if (startValue >= json.Length) return null;

        if (json[startValue] == '"')
        {
            var endValue = json.IndexOf('"', startValue + 1);
            return endValue == -1 ? null : json.Substring(startValue, endValue - startValue + 1);
        }
        else
        {
            var endValue = startValue;
            while (endValue < json.Length && json[endValue] != ',' && json[endValue] != '}' && json[endValue] != ']')
                endValue++;
            return json.Substring(startValue, endValue - startValue);
        }
    }

    private string CleanValue(string val) => val.Trim('"');   
}