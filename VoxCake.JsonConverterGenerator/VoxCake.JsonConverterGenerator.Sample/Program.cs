using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VoxCake.JsonConverterGenerator.Sample
{
    public class Program
    {
        private const string SampleJsonPath = "C:/github/json-converter-generator/VoxCake.JsonConverterGenerator/VoxCake.JsonConverterGenerator.Sample/SerializedContract.json";
    
        public static void Main()
        {
            var productJson = File.ReadAllText(SampleJsonPath);

            var obj = CreateProduct();
        
            var generatedProductJson = JsonConvert.SerializeObject(obj);
            
            CompareStrings(JToken.Parse(productJson).ToString(), JToken.Parse(generatedProductJson).ToString());
            
            var productValue = JsonConvert.DeserializeObject<Product>(productJson);
        }
        
        public static void CompareStrings(string expected, string actual)
        {
            if (expected == actual)
            {
                return;
            }
            else
            {
                string difference = GetDifference(expected, actual);
                throw new Exception($"Строки не совпадают:\n{difference}");
            }
        }
        
        private static string GetDifference(string expected, string actual)
        {
            var diffMessage = new StringBuilder();
            int minLength = Math.Min(expected.Length, actual.Length);

            for (int i = 0; i < minLength; i++)
            {
                if (expected[i] != actual[i])
                {
                    diffMessage.AppendLine($"Различие на позиции {i}:");
                    diffMessage.AppendLine($"Ожидалось: '{expected[i]}'");
                    diffMessage.AppendLine($"Фактически: '{actual[i]}'");
                    return diffMessage.ToString();
                }
            }

            if (expected.Length != actual.Length)
            {
                diffMessage.AppendLine("Строки имеют разную длину.");
                diffMessage.AppendLine($"Ожидалось длина: {expected.Length}");
                diffMessage.AppendLine($"Фактически длина: {actual.Length}");
                return diffMessage.ToString();
            }

            diffMessage.AppendLine("Строки различаются, но место отличия не найдено.");
            return diffMessage.ToString();
        }
    

        private static Product CreateProduct()
        {
            return new Product
            {
                Id = 12345,
                Name = "Продукт",
                Price = 99.99,
                Categories = new List<Category>
                {
                    new Category
                    {
                        Id = 1,
                        Name = "Электроника",
                        Subcategories = new List<Subcategory>
                        {
                            new Subcategory { Id = 10, Name = "Смартфоны" },
                            new Subcategory { Id = 11, Name = "Ноутбуки" }
                        }
                    },
                    new Category
                    {
                        Id = 2,
                        Name = "Бытовая техника",
                        Subcategories = new List<Subcategory>
                        {
                            new Subcategory { Id = 20, Name = "Холодильники" },
                            new Subcategory { Id = 21, Name = "Стиральные машины" }
                        }
                    }
                },
                Availability = new Availability
                {
                    Online = true,
                    Stores = new List<Store>
                    {
                        new Store { Id = 100, Name = "Магазин на Ленинском", Stock = 5 },
                        new Store { Id = 101, Name = "Магазин на Тверской", Stock = 0 }
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
                    Features = new List<string> { "Bluetooth", "Wi-Fi", "GPS" }
                },
                Reviews = new List<Review>
                {
                    new Review
                    {
                        User = "Иван Иванов",
                        Rating = 5,
                        Comment = "Отличный продукт!"
                    },
                    new Review
                    {
                        User = "Петр Петров",
                        Rating = 4,
                        Comment = "Хорошо, но есть недочеты"
                    }
                }
            };
        }
    }
}