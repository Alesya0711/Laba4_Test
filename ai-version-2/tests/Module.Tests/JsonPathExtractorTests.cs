using NUnit.Framework;
using Module.Core;

namespace Tests;
 
[TestFixture]
public class JsonPathExtractorTests
{
    private IJsonPathExtractor _extractor;

    [SetUp]
    public void Setup()
    {
        _extractor = new JsonPathExtractor();
    }

    [Test]
    [Description("TC-01: Простой ключ - позитивный тест")]
    public void ExtractValue_SimpleKey_ReturnsValue()
    {
        string json = "{\"name\":\"John\"}";
        string path = "$.name";
        string actual = _extractor.ExtractValue(json, path);
        Assert.That(actual, Is.EqualTo("John"));
    }

    [Test]
    [Description("TC-02: Вложенный ключ - позитивный тест")]
    public void ExtractValue_NestedKey_ReturnsValue()
    {
        string json = "{\"user\":{\"name\":\"Alice\"}}";
        string path = "$.user.name";
        string actual = _extractor.ExtractValue(json, path);
        Assert.That(actual, Is.EqualTo("Alice"));
    }

    [Test]
    [Description("TC-03: Ключ не найден - негативный тест")]
    public void ExtractValue_KeyNotFound_ReturnsNull()
    {
        string json = "{\"name\":\"John\"}";
        string path = "$.age";
        string actual = _extractor.ExtractValue(json, path);
        Assert.That(actual, Is.Null);
    }

    [Test]
    [Description("TC-04: Пустой JSON - негативный тест")]
    public void ExtractValue_EmptyJson_ReturnsNull()
    {
        string json = "";
        string path = "$.name";
        string actual = _extractor.ExtractValue(json, path);
        Assert.That(actual, Is.Null);
    }

    [Test]
    [Description("TC-05: Null JSON - негативный тест")]
    public void ExtractValue_NullJson_ReturnsNull()
    {
        string? json = null;
        string path = "$.name";
        Assert.DoesNotThrow(() => _extractor.ExtractValue(json, path));
        Assert.That(_extractor.ExtractValue(json, path), Is.Null);
    }

    [Test]
    [Description("TC-06: Null path - негативный тест")]
    public void ExtractValue_NullPath_ReturnsNull()
    {
        string json = "{\"name\":\"John\"}";
        string? path = null;
        Assert.DoesNotThrow(() => _extractor.ExtractValue(json, path));
        Assert.That(_extractor.ExtractValue(json, path), Is.Null);
    }

    [Test]
    [Description("TC-07: Числовое значение - граничный тест")]
    public void ExtractValue_NumericValue_NoQuotes()
    {
        string json = "{\"count\": 100}";
        string path = "$.count";
        string actual = _extractor.ExtractValue(json, path);
        Assert.That(actual, Is.EqualTo("100"));
    }

    [Test]
    [Description("TC-08: Булево значение - граничный тест")]
    public void ExtractValue_BooleanValue_NoQuotes()
    {
        string json = "{\"active\": true}";
        string path = "$.active";
        string actual = _extractor.ExtractValue(json, path);
        Assert.That(actual, Is.EqualTo("true"));
    }

    [Test]
    [Description("TC-09: Одинаковые ключи на разных уровнях - белый ящик")]
    public void ExtractValue_SimilarKeys_DistinguishesLevels()
    {
        string json = "{\"data\":{\"id\": 1},\"id\": 2}";
        string path = "$.data.id";
        string actual = _extractor.ExtractValue(json, path);
        Assert.That(actual, Is.EqualTo("1"));
    }

    [Test]
    [Description("TC-10: Последний ключ без запятой - белый ящик")]
    public void ExtractValue_LastKeyNoComma_HandlesEnd()
    {
        string json = "{\"last\":\"value\"}";
        string path = "$.last";
        string actual = _extractor.ExtractValue(json, path);
        Assert.That(actual, Is.EqualTo("value"));
    }

    [Test]
    [Description("TC-11: Глубокая вложенность - белый ящик")]
    public void ExtractValue_DeepNesting_Works()
    {
        string json = "{\"a\":{\"b\":{\"c\":{\"d\":\"deep\"}}}}";
        string path = "$.a.b.c.d";
        string actual = _extractor.ExtractValue(json, path);
        Assert.That(actual, Is.EqualTo("deep"));
    }

    [Test]
    [Description("TC-12: Пустое значение - граничный тест")]
    public void ExtractValue_EmptyValue_ReturnsEmptyString()
    {
        string json = "{\"key\":\"\"}";
        string path = "$.key";
        string actual = _extractor.ExtractValue(json, path);
        Assert.That(actual, Is.EqualTo(""));
    }

    [Test]
    [Description("TC-13: Пустой путь - негативный тест")]
    public void ExtractValue_EmptyPath_ReturnsNull()
    {
        string json = "{\"name\":\"John\"}";
        string path = "";
        string actual = _extractor.ExtractValue(json, path);
        Assert.That(actual, Is.Null);
    }

    [Test]
    [Description("TC-14: Пробелы в JSON - белый ящик")]
    public void ExtractValue_JsonWithSpaces_Works()
    {
        string json = "{ \"name\" : \"John\" }";
        string path = "$.name";
        string actual = _extractor.ExtractValue(json, path);
        Assert.That(actual, Is.EqualTo("John"));
    }

    [Test]
    [Description("TC-15: Null значение в JSON - граничный тест")]
    public void ExtractValue_NullValueInJson_ReturnsNull()
    {
        string json = "{\"data\": null}";
        string path = "$.data";
        string actual = _extractor.ExtractValue(json, path);
        Assert.That(actual, Is.EqualTo("null"));
    }
}