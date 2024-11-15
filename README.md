# JsonBaker

![main github action workflow](https://github.com/imkoi/json-baker/actions/workflows/dotnet.yml/badge.svg) [![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## What is JsonBaker?

JsonBaker is library designed to boost the performance of Newtonsoft.Json serialization and deserialization. It use
Roslyn source generation to create optimized JsonConverter implementations for specific types.

Depending on the use case, JsonBaker can enhance the performance of JSON operations by 2 to 8 times, reducing overhead
and providing faster, more efficient handling of JSON data.

## What benefits it give

The JsonBaker boost in serialization and deserialization speed by minimizing reliance on reflection.
It pre-compiles the serialization flow. This approach gives direct, optimized code paths, resulting in faster and more efficient data processing.

**PC** Benchmark:

| Metric                       | Default      | JsonBaker   | Difference       |
|------------------------------|--------------|-------------|------------------|
| Serialization Time [AVG]     | **0**,14 ms  | **0**,01 ms | **7**.42x Faster |
| Serialization Time [TOTAL]   | **14**,30 ms | **1**,92 ms | **7**.42x Faster |
| Deserialization Time [AVG]   | **0**,21 ms  | **0**,03 ms | **5**.65x Faster |
| Deserialization Time [TOTAL] | **21**,65 ms | **3**,83 ms | **5**.65x Faster |

**Android** Benchmark:

| Metric                       | Default        | JsonBaker    | Difference        |
|------------------------------|----------------|--------------|-------------------|
| Serialization Time [AVG]     | 	**0**,71 ms   | **0**,10 ms  | **7**.48x Faster  |
| Serialization Time [TOTAL]   | **71**,17 ms   | **9**,52 ms  | **7**.48x Faster  |
| Deserialization Time [AVG]   | 	**1**,03 ms   | **0**,18 ms  | **5**.84x Faster  |
| Deserialization Time [TOTAL] | 	**102**,81 ms | **17**,62 ms | 	**5**.84x Faster |

[Json used in benchmark](https://github.com/imkoi/json-baker/blob/main/VoxCake.JsonBaker.Sample/Benchmark.json)

## How to install

Install as GIT dependency via Package Manager

1. Install [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) for nuget management
2. Open NuGet Manager window (NuGet → Manage NuGet Packages)
3. Install Microsoft.CodeAnalysis.CSharp 3.8.0 nuget
4. Open Package Manager window (Window → Package Manager)
5. Click `+` button on the upper-left of a window, and select "Add package from git URL..."
6. Enter the following URL and click `Add` button

```
https://github.com/imkoi/json-baker.git?path=/VoxCake.JsonBaker/Package
```

# How to use

Let’s say you have a simple data model like this, but you're noticing slow deserialization performance:

```csharp
// Contract
public class Review
{
    public string User { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
}

// Usage
JsonConvert.DeserializeObject<Review>(reviewJson);
```

With JsonBaker, boosting performance is straightforward. Just follow these steps:

```csharp
// Reference VoxCake.JsonBaker
using VoxCake.JsonBaker;

// Add the [JsonBaker] attribute to indicate that this class should have an optimized JSON converter generated.
[JsonBaker]
public class Review
{
    public string User { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
}

// When serializing / deserializing, use JsonBakerSettings.Default to apply the generated converter.
JsonConvert.DeserializeObject<Review>(reviewJson, JsonBakerSettings.Default);
```

## How it works

JsonBaker optimizes JSON serialization and deserialization by generating converters for specific types and reducing
reliance on reflection. Here’s how JsonBaker achieves these performance improvements:

1. Converter Generation: JsonBaker automatically generates converters for classes marked with the [JsonBaker] attribute,
   creating efficient converters tailored to each type.
2. Assembly-Level Converter Provider: For each assembly, JsonBaker generates a converter provider that supplies the
   correct converter for each specific type.
3. Using JsonBakerSettings: At runtime, when you apply JsonBakerSettings during serialization or deserialization, a
   global converter is used to apply the optimizations.
4. Efficient Caching: The global converter uses reflection only once to retrieve converter providers, then caches them
   for fast access, minimizing the performance cost of reflection on subsequent operations.

### Converter per concrete type

```csharp
using System;
using Newtonsoft.Json;
using VoxCake.JsonBaker.Sample;

public class ReviewConverter_Generated : JsonConverter
{
  public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
  {
    Review concreteValue = (Review)value;
    writer.WriteStartObject();
    writer.WritePropertyName("User");
    writer.WriteValue(concreteValue.User);
    writer.WritePropertyName("Rating");
    writer.WriteValue(concreteValue.Rating);
    writer.WritePropertyName("Comment");
    writer.WriteValue(concreteValue.Comment);
    writer.WriteEndObject();
  }

  public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
  {
    if ((int)reader.TokenType == 11)
    {
      return null;
    }
    Review value = new Review();
    reader.Read();
    while ((int)reader.TokenType == 4)
    {
      string propertyName = (string)reader.Value;
      reader.Read();
      switch (propertyName)
      {
        case "User":
          value.User = (string)reader.Value;
          break;
        case "Rating":
          value.Rating = (reader.Value != null) ? Convert.ToInt32(reader.Value) : 0;
          break;
        case "Comment":
          value.Comment = (string)reader.Value;
          break;
        default:
          if (propertyName.Equals("User", StringComparison.OrdinalIgnoreCase))
          {
            value.User = (string)reader.Value;
          }
          else if (propertyName.Equals("Rating", StringComparison.OrdinalIgnoreCase))
          {
            value.Rating = (reader.Value != null) ? Convert.ToInt32(reader.Value) : 0;
          }
          else if (propertyName.Equals("Comment", StringComparison.OrdinalIgnoreCase))
          {
            value.Comment = (string)reader.Value;
          }
          else
          {
            reader.Skip();
          }
          break;
      }
      reader.Read();
    }
    return value;
  }

  public override bool CanConvert(Type objectType)
  {
    return true;
  }
}

```

### Converter provider per assembly

```csharp
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VoxCake.JsonBaker;
using VoxCake.JsonBaker.Sample;

public class JsonBakerAssemblyConverterProvider : JsonBakerAssemblyConverterProviderBase
{
  private Dictionary<Type, JsonConverter> _converters;

  private bool _initialized;

  public override JsonConverter GetConverter(Type type)
  {
    if (!_initialized)
    {
      Initialize();
      _initialized = true;
    }
    _converters.TryGetValue(type, out var converter);
    return converter;
  }

  private void Initialize()
  {
    _converters = new Dictionary<Type, JsonConverter>(16)
    {
      { typeof(Review), (JsonConverter)(object)new ReviewConverter_Generated() }
    };
  }
}

```

## Limitations

JsonBaker leverages code generation at compile time, but this also imposes certain limitations.
Since the functionality is constrained by the capabilities of a source generator, not all types can be serialized, and not all JSON attributes are supported.
However, this limitation is intentional: if a data type does not meet the requirements for code generation, the compiler will raise an error, allowing you to catch issues before Runtime.
This ensures that **you cannot compile a class that is unstable or unsupported**, enhancing the reliability and predictability of your code.

Supported Attributes:
- JsonProperty
- - PropertyName
- - NullValueHandling
- JsonIgnore

Future Supported Attributes:
- JsonProperty
- - Order
- - Required
- - DefaultValueHandling
- JsonRequired
- JsonConverter

Moreover, JsonBaker provide reflection free creation of collections and deserializing items to them. It gives additional performance benefits, 
but still not all types are supported. If collection type are not supported - default json serialization with reflection will be used.

Collections Supported For Fast Processing:
- List
- Queue
- Stack
- HashSet

Future Collections Supported For Fast Processing:
- Dictionary
- Array