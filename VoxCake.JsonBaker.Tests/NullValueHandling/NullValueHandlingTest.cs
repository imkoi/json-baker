using FluentAssertions;
using Newtonsoft.Json;

namespace VoxCake.JsonBaker.Tests;

public class NullValueHandlingTest
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
        yield return "Empty.json";
        yield return "AllNotNull.json";
        
        yield return "IncludedAndIgnoredAreNull.json";
        yield return "IncludedAreNull.json";
        yield return "IgnoredAreNull.json";
        
        yield return "IncludedAndIgnoredAreEmpty.json";
        yield return "IncludedAreEmpty.json";
        yield return "IgnoredAreEmpty.json";
    }
    
    [JsonBaker]
    public class TestObject
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string IncludedProperty { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string IgnoredProperty { get; set; }

        public string RegularProperty { get; set; }
    }
}