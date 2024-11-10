using System.Collections.Generic;
using Newtonsoft.Json;

namespace VoxCake.JsonBaker.Sample
{
    [JsonBaker]
    public class Product
    {
        [JsonIgnore]
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
        [JsonProperty("name")]
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
