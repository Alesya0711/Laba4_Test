namespace Module.Core;

public class JsonPathExtractor : IJsonPathExtractor
{
    public string? ExtractValue(string json, string path)
    {
        var keys = path.Split('.');
        string temp = json;

        foreach (var k in keys)
        {
            var keyName = k.Replace("$", "");
            var start = temp.IndexOf($"\"{keyName}\"");  
            if (start == -1) return null;

            var valStart = temp.IndexOf(':', start) + 1;
            var valEnd = temp.IndexOf(',', valStart);
            if (valEnd == -1) valEnd = temp.IndexOf('}', valStart);
            if (valEnd == -1) return null;

            temp = temp.Substring(valStart, valEnd - valStart).Trim();
        }

        return temp.Trim('"');
    }
}