using FluentAssertions;
using Newtonsoft.Json;

namespace VoxCake.JsonBaker.Tests;

public class CollectionsTest
{
    [TestCaseSource(nameof(GetTestData))]
    public void Deserialization_GivesEquivalentObjects(string jsonName)
    {
        var referenceJson = TestUtility.GetJson(this, jsonName);
        
        var referenceObject = JsonConvert.DeserializeObject<TestObject>(referenceJson);
        var resultObject = JsonConvert.DeserializeObject<TestObject>(referenceJson, JsonBakerSettings.Default);

        resultObject.Should().BeEquivalentTo(referenceObject);
    }
    
    [TestCaseSource(nameof(GetTestData))]
    public void Serialization_GivesEquivalentJsons(string jsonName)
    {
        var referenceObject = TestUtility.GetJson<TestObject>(this, jsonName);
        
        var referenceJson = JsonConvert.SerializeObject(referenceObject);
        var resultJson = JsonConvert.SerializeObject(referenceObject, JsonBakerSettings.Default);

        resultJson.Should().BeEquivalentTo(referenceJson);
    }

    private static IEnumerable<string> GetTestData()
    {
        yield return "AllDefaults.json";
        yield return "AllNotNull.json";
        yield return "AllNulls.json";
        yield return "Empty.json";
        yield return "IgnoredOnly.json";
        yield return "IncludedOnly.json";
        yield return "MissingProperties.json";
        yield return "PopulatedOnly.json";
    }
    
    [JsonBaker]
    public class TestObject
    {
        public List<string> List { get; set; }
        public LinkedList<string> LinkedListProperty { get; set; }
        public Stack<string> StackProperty { get; set; }
        public Queue<string> Queue { get; set; }
        public HashSet<string> Hashset { get; set; }
        public Dictionary<string, string> Dictionary { get; set; }
        public string[] Array { get; set; }
    }
}