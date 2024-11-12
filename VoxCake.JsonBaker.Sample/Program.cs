using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VoxCake.JsonBaker.Sample
{
    public class Program
    {
        private const string SampleJsonPath = "C:/github/json-converter-generator/VoxCake.JsonBaker.Sample/SerializedContract.json";
    
        public static void Main()
        {
            var productJson = File.ReadAllText(SampleJsonPath);

            JsonBakerSettings.WarningReceived += warning =>
            {
                Console.WriteLine(warning);
            };

            var obj = CreateProduct();
        
            var generatedProductJson = JsonConvert.SerializeObject(obj, JsonBakerSettings.Default);

            if (JToken.Parse(productJson).ToString() != JToken.Parse(generatedProductJson).ToString())
            {
                throw new Exception("Wrong json");
            }
            
            var productValue = JsonConvert.DeserializeObject<Product>(productJson);
            
            JsonConvert.PopulateObject(generatedProductJson, productValue, JsonBakerSettings.Default);
        }

        private static Product CreateProduct()
        {
            return new Product
            {
                Id = 12345,
                Name = "Product",
                Price = 99.99,
                Categories = new List<Category>
                {
                    new Category
                    {
                        Id = 1,
                        Name = "Placeholder",
                        Subcategories = new List<Subcategory>
                        {
                            new Subcategory { Id = 10, Name = "Placeholder" },
                            new Subcategory { Id = 11, Name = "Placeholder" }
                        }
                    },
                    new Category
                    {
                        Id = 2,
                        Name = "Placeholder",
                        Subcategories = new List<Subcategory>
                        {
                            new Subcategory { Id = 20, Name = "Placeholder" },
                            new Subcategory { Id = 21, Name = "Placeholder" }
                        }
                    }
                },
                Availability = new Availability
                {
                    Online = true,
                    Stores = new List<Store>
                    {
                        new Store { Id = 100, Name = "Placeholder", Stock = 5 },
                        new Store { Id = 101, Name = "Placeholder", Stock = 0 }
                    }
                },
                Specifications = new Specifications
                {
                    Weight = "1kg",
                    Dimensions = new Dimensions
                    {
                        Width = 10,
                        Height = 20,
                        Depth = 2
                    },
                    Features = new List<string> { "Placeholder", "Placeholder", "Placeholder" }
                },
                Reviews = new List<Review>
                {
                    new Review
                    {
                        User = "Placeholder",
                        Rating = 5,
                        Comment = "Placeholder!"
                    },
                    new Review
                    {
                        User = "Placeholder",
                        Rating = 4,
                        Comment = "Placeholder, but placeholder!"
                    }
                }
            };
        }
    }
}