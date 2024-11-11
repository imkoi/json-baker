using System.ComponentModel;
using FluentAssertions;
using Newtonsoft.Json;

namespace VoxCake.JsonBaker.Tests;

public class PropertyNameTest
{
    [TestCaseSource(nameof(GetTestData))]
    public void Deserialization_GivesEquivalentObjects(string jsonName)
    {
        var referenceJson = TestUtility.GetJson(this, jsonName);
        
        var referenceObject = JsonConvert.DeserializeObject<TestObject>(referenceJson);
        var resultObject = JsonConvert.DeserializeObject<TestObject>(referenceJson, JsonBakerSettings.Default);

        referenceObject.Should().BeEquivalentTo(resultObject);
    }
    
    [TestCaseSource(nameof(GetTestData))]
    public void Serialization_GivesEquivalentJsons(string jsonName)
    {
        var referenceObject = TestUtility.GetJson<TestObject>(this, jsonName);
        
        var referenceJson = JsonConvert.SerializeObject(referenceObject);
        var resultJson = JsonConvert.SerializeObject(referenceObject, JsonBakerSettings.Default);

        referenceJson.Should().BeEquivalentTo(resultJson);
    }

    private static IEnumerable<string> GetTestData()
    {
        yield return "Case1.json";
        yield return "Case2.json";
        yield return "Case3.json";
        yield return "Case4.json";
        yield return "Case5.json";
    }
    
    [JsonBaker]
    public class TestObject
    {
        [JsonProperty("custom_name")]
        public string CustomNameProperty { get; set; }

        [JsonProperty("different_case")]
        public string DifferentCaseProperty { get; set; }

        [JsonProperty(PropertyName = "with_special_chars")]
        public string SpecialCharsProperty { get; set; }

        [JsonProperty("nested_property")]
        public string NestedProperty { get; set; }

        [JsonProperty(PropertyName = "simple_property")]
        public string SimpleProperty { get; set; }

        public string IgnoredProperty { get; set; }
    }
}