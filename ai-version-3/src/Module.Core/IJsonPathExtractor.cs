namespace Module.Core;

public interface IJsonPathExtractor
{ 
    string? ExtractValue(string json, string path);
} 