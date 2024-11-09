using System.Collections.Generic;
using Newtonsoft.Json;

namespace VoxCake.JsonBaker.Sample
{
    //[JsonConverter(typeof(ProductConverter_Generated))]
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

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Subcategory> Subcategories { get; set; }
    }

    public class Subcategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Availability
    {
        public bool Online { get; set; }
        public List<Store> Stores { get; set; }
    }

    public class Store
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Stock { get; set; }
    }

    public class Specifications
    {
        public string Weight { get; set; }
        public Dimensions Dimensions { get; set; }
        public List<string> Features { get; set; }
    }

    public class Dimensions
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Depth { get; set; }
    }

    public class Review
    {
        public string User { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
