using System;
using Microsoft.CodeAnalysis;

namespace VoxCake.JsonBaker.SourceGenerator;

public static class JsonBakerConverterGenerator
{
    public static (string typeName, string converterName) Generate(ITypeSymbol typeSymbol, CodeWriter writer)
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

                foreach (var member in typeSymbol.GetSerializableMembers())
                {
                    var propertyName = member.name;

                    writer.WriteLine($"writer.WritePropertyName(nameof(concreteValue.{propertyName}));");

                    if (member.type.IsPrimitiveType())
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
                        foreach (var member in typeSymbol.GetSerializableMembers())
                        {
                            var readValueCode = member.type.GetReadValueCode();
                            
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
}