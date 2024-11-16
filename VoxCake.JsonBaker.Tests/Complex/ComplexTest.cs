using FluentAssertions;
using Newtonsoft.Json;

namespace VoxCake.JsonBaker.Tests;

public class ComplexTest
{
    [TestCaseSource(nameof(GetTestData))]
    public void Deserialization_GivesEquivalentObjects(string jsonName)
    {
        var referenceJson = TestUtility.GetJson(this, jsonName);
        
        var referenceObject = JsonConvert.DeserializeObject<Product>(referenceJson);
        var resultObject = JsonConvert.DeserializeObject<Product>(referenceJson, JsonBakerSettings.Default);

        resultObject.Should().BeEquivalentTo(referenceObject);
    }
    
    [TestCaseSource(nameof(GetTestData))]
    public void Serialization_GivesEquivalentJsons(string jsonName)
    {
        var referenceObject = TestUtility.GetJson<Product>(this, jsonName);
        
        var referenceJson = JsonConvert.SerializeObject(referenceObject);
        var resultJson = JsonConvert.SerializeObject(referenceObject, JsonBakerSettings.Default);

        resultJson.Should().BeEquivalentTo(referenceJson);
    }

    private static IEnumerable<string> GetTestData()
    {
        yield return "SerializedContract.json";
    }
    
    [JsonBaker]
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public List<Category> Categories { get; set; }
        public Availability Availability { get; set; }
        public Specifications Specifications { get; set; }
        public List<Review> Reviews { get; set; }
    }

    [JsonBaker]
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Subcategory> Subcategories { get; set; }
    }

    [JsonBaker]
    public class Subcategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [JsonBaker]
    public class Availability
    {
        public bool Online { get; set; }
        public List<Store> Stores { get; set; }
    }

    [JsonBaker]
    public class Store
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Stock { get; set; }
    }

    [JsonBaker]
    public class Specifications
    {
        public string Weight { get; set; }
        public Dimensions Dimensions { get; set; }
        public List<string> Features { get; set; }
    }

    [JsonBaker]
    public class Dimensions
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Depth { get; set; }
    }

    [JsonBaker]
    public class Review
    {
        public string User { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}