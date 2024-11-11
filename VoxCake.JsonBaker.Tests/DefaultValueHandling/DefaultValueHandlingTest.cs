// using System.ComponentModel;
// using FluentAssertions;
// using Newtonsoft.Json;
//
// namespace VoxCake.JsonBaker.Tests;
//
// public class DefaultValueHandlingTest
// {
//     [TestCaseSource(nameof(GetTestData))]
//     public void Deserialization_GivesEquivalentObjects(string jsonName)
//     {
//         var referenceJson = TestUtility.GetJson(this, jsonName);
//         
//         var referenceObject = JsonConvert.DeserializeObject<TestObject>(referenceJson);
//         var resultObject = JsonConvert.DeserializeObject<TestObject>(referenceJson, JsonBakerSettings.Default);
//
//         referenceObject.Should().BeEquivalentTo(resultObject);
//     }
//     
//     [TestCaseSource(nameof(GetTestData))]
//     public void Serialization_GivesEquivalentJsons(string jsonName)
//     {
//         var referenceObject = TestUtility.GetJson<TestObject>(this, jsonName);
//         
//         var referenceJson = JsonConvert.SerializeObject(referenceObject);
//         var resultJson = JsonConvert.SerializeObject(referenceObject, JsonBakerSettings.Default);
//
//         referenceJson.Should().BeEquivalentTo(resultJson);
//     }
//
//     private static IEnumerable<string> GetTestData()
//     {
//         yield return "AllDefaults.json";
//         yield return "AllNotNull.json";
//         yield return "AllNulls.json";
//         yield return "Empty.json";
//         yield return "IgnoredOnly.json";
//         yield return "IncludedOnly.json";
//         yield return "MissingProperties.json";
//         yield return "PopulatedOnly.json";
//     }
//     
//     [JsonBaker]
//     public class TestObject
//     {
//         [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
//         [DefaultValue(42)]
//         public int IncludedProperty { get; set; }
//
//         [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
//         [DefaultValue("Default String")]
//         public string IgnoredProperty { get; set; }
//         
//         [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
//         [DefaultValue("Populated String")]
//         public string PopulatedStringProperty { get; set; }
//
//         [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
//         [DefaultValue(true)]
//         public bool PopulatedProperty { get; set; }
//
//         [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
//         [DefaultValue(3.14)]
//         public double IgnoredAndPopulatedProperty { get; set; }
//
//         public DateTime RegularDateTime { get; set; } = DateTime.MinValue;
//     }
// }