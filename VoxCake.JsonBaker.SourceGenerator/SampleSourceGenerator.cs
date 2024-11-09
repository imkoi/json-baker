using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace VoxCake.JsonBaker.SourceGenerator;

[Generator]
public class SampleSourceGenerator : ISourceGenerator
{
    private readonly HashSet<string> _primitiveTypes = new HashSet<string>
    {
        "string",
        "string?",
        "int",
        "uint",
        "long",
        "ulong",
        "float",
        "double",
        "bool",
        "short",
        "ushort",
        "char",
        "byte",
        "sbyte",
        "decimal",
        "DateTime",
        "DateTimeOffset",
        "Guid",
        "TimeSpan",
        "int?",
        "uint?",
        "long?",
        "ulong?",
        "float?",
        "double?",
        "bool?",
        "short?",
        "ushort?",
        "char?",
        "byte?",
        "sbyte?",
        "decimal?",
        "DateTime?",
        "DateTimeOffset?",
        "Guid?",
        "TimeSpan?",
        "byte[]?",
        "byte[]",
        "Uri?",
        "Uri"
    };

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SerializableTypesReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (!(context.SyntaxContextReceiver is SerializableTypesReceiver receiver))
            return;

        if (receiver.SerializableTypes.Count < 1)
        {
            return;
        }

        var codeWriter = new CodeWriter(4, "System", "System.Collections.Generic", "Newtonsoft.Json");
        var converterNames = new List<(string typeName, string converterName)>(receiver.SerializableTypes.Count);
        
        foreach (var typeSymbol in receiver.SerializableTypes)
        {
            var converterName = GenerateConverter(typeSymbol, codeWriter);
            
            converterNames.Add(converterName);
        }
        
        using(codeWriter.Scope("public class JsonBakerAssemblyConverter : JsonConverter"))
        {
            codeWriter.WriteLine("private Dictionary<Type, JsonConverter> _converters;");
            codeWriter.WriteLine("private bool _initialized;");
            codeWriter.EmptyLine();
            
            using(codeWriter.Scope("public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)"))
            {
                codeWriter.WriteLine("_converters[value.GetType()].WriteJson(writer, value, serializer);");
            }
            
            using(codeWriter.Scope("public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)"))
            {
                codeWriter.WriteLine("return _converters[objectType].ReadJson(reader, objectType, existingValue, serializer);");
            }
            
            using(codeWriter.Scope("public override bool CanConvert(Type objectType)"))
            {
                using (codeWriter.Scope("if (!_initialized)"))
                {
                    codeWriter.WriteLine("Initialize();");
                    codeWriter.WriteLine("_initialized = true;");
                }
                
                codeWriter.WriteLine("return _converters.TryGetValue(objectType, out JsonConverter converter) && converter.CanConvert(objectType);");
            }
            
            using(codeWriter.Scope("private void Initialize()"))
            {
                using (codeWriter.Scope("_converters = new Dictionary<Type, JsonConverter>(16)", ";"))
                {
                    var index = 0;
                    
                    foreach (var converter in converterNames)
                    {
                        var inner = $"typeof({converter.typeName}), new {converter.converterName}()";
                        var endSymbol = index + 1 < converterNames.Count ? "," : "";
                        
                        codeWriter.WriteLine("{ " + inner + " }" + endSymbol);

                        index++;
                    }
                }
            }
        }
        
        var generatedCode = codeWriter.Build();
        
        context.AddSource("JsonBakerAssemblyConverter.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
    }

    private (string typeName, string converterName) GenerateConverter(INamedTypeSymbol typeSymbol, CodeWriter writer)
    {
        var resultConverterName = string.Empty;
        var resultTypeName = string.Empty;
        var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
        var className = typeSymbol.Name;
        var converterName = $"{className}Converter_Generated";

        var namespaceScope = default(IDisposable);
        
        if (!string.IsNullOrEmpty(namespaceName))
        {
            resultConverterName += namespaceName + ".";
            resultTypeName += namespaceName + ".";
            
            namespaceScope = writer.Scope($"namespace {namespaceName}");
        }

        resultConverterName += converterName;
        resultTypeName += className;

        using (writer.Scope($"public class {converterName} : JsonConverter"))
        {
            using(writer.Scope("public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)"))
            {
                writer.WriteLine($"var concreteValue = ({className}) value;");
                
                writer.WriteLine("writer.WriteStartObject();");

                foreach (var member in GetSerializableMembers(typeSymbol))
                {
                    var propertyName = member.name;

                    writer.WriteLine($"writer.WritePropertyName(nameof(concreteValue.{propertyName}));");

                    if (IsPrimitiveType(member.type))
                    {
                        writer.WriteLine($"writer.WriteValue(concreteValue.{propertyName});");
                    }
                    else
                    {
                        writer.WriteLine($"serializer.Serialize(writer, concreteValue.{propertyName});");
                    }
                }
                
                writer.WriteLine("writer.WriteEndObject();");
            }
            
            using(writer.Scope("public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)"))
            {
                using (writer.Scope("if (reader.TokenType == JsonToken.Null)"))
                {
                    writer.WriteLine("return null;");
                }
                
                writer.WriteLine($"var value = new {className}();");
                writer.WriteLine("reader.Read();");
                writer.EmptyLine();

                using (writer.Scope("while (reader.TokenType == JsonToken.PropertyName)"))
                {
                    writer.WriteLine("var propertyName = (string)reader.Value;");
                    writer.WriteLine("reader.Read();");
                    writer.WriteLine("");

                    using (writer.Scope("switch (propertyName)"))
                    {
                        foreach (var member in GetSerializableMembers(typeSymbol))
                        {
                            var readValueCode = GetReadValueCode(member.type);
                            
                            writer.WriteLine($"case nameof(value.{member.name}):");
                            writer.WriteLine($"    value.{member.name} = {readValueCode};");
                            writer.WriteLine($"    break;");
                        }
                        
                        writer.WriteLine("default:");
                        writer.WriteLine("    reader.Skip();");
                        writer.WriteLine("    break;");
                    }
                    
                    writer.WriteLine("reader.Read();");
                }
                
                writer.WriteLine("return value;");
            }
            
            using(writer.Scope("public override bool CanConvert(Type objectType)"))
            {
                writer.WriteLine("return true;");
            }
        }
        
        namespaceScope?.Dispose();

        return (resultTypeName, resultConverterName);
    }

    private IEnumerable<(string name, ITypeSymbol type)> GetSerializableMembers(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IPropertySymbol property && !property.IsStatic && property.SetMethod != null)
            {
                yield return (property.Name, property.Type);
            }
            else if (member is IFieldSymbol field && !field.IsStatic)
            {
                var fieldName = field.Name;

                if (!fieldName.EndsWith(">k__BackingField"))
                {
                    yield return (fieldName, field.Type);
                }
            }
        }
    }

    private string GetReadValueCode(ITypeSymbol typeSymbol)
    {
        if (IsPrimitiveType(typeSymbol))
        {
            var typeName = typeSymbol.ToDisplayString();

            switch (typeName)
            {
                case "string":
                case "string?":
                    return "(string)reader.Value";
                case "int":
                case "int?":
                    return "reader.Value != null ? Convert.ToInt32(reader.Value) : default";
                case "long":
                case "long?":
                    return "reader.Value != null ? Convert.ToInt64(reader.Value) : default";
                case "float":
                case "float?":
                    return "reader.Value != null ? Convert.ToSingle(reader.Value) : default";
                case "double":
                case "double?":
                    return "reader.Value != null ? Convert.ToDouble(reader.Value) : default";
                case "bool":
                case "bool?":
                    return "reader.Value != null ? Convert.ToBoolean(reader.Value) : default";
                case "DateTime":
                case "DateTime?":
                    return "reader.Value != null ? Convert.ToDateTime(reader.Value) : default";
                default:
                    return $"({typeName})reader.Value";
            }
        }
        else
        {
            return $"serializer.Deserialize<{typeSymbol.ToDisplayString()}>(reader)";
        }
    }

    private bool IsPrimitiveType(ITypeSymbol typeSymbol)
    {
        var displayString = typeSymbol.ToDisplayString();

        return _primitiveTypes.Contains(displayString);
    }
}
