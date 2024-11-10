using System;
using System.Linq;
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
                    if (!member.info.IncludeDefaultValues || !member.info.IncludeNullValues)
                    {
                        using (writer.Scope($"if (concreteValue.{member.memberName} != default)"))
                        {
                            writer.WriteLine($"writer.WritePropertyName(\"{member.info.PropertyName}\");");
                        
                            if (member.memberType.IsPrimitiveType())
                            {
                                writer.WriteLine($"writer.WriteValue(concreteValue.{member.memberName});");
                            }
                            else
                            {
                                writer.WriteLine($"serializer.Serialize(writer, concreteValue.{member.memberName});");
                            }
                        }
                    }
                    else
                    {
                        writer.WriteLine($"writer.WritePropertyName(\"{member.info.PropertyName}\");");
                        
                        if (member.memberType.IsPrimitiveType())
                        {
                            writer.WriteLine($"writer.WriteValue(concreteValue.{member.memberName});");
                        }
                        else
                        {
                            writer.WriteLine($"serializer.Serialize(writer, concreteValue.{member.memberName});");
                        }
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

                    // var serializableMembers = typeSymbol.GetSerializableMembers().ToArray();
                    //
                    // if (serializableMembers.Length == 1)
                    // {
                    //     var singleMember = serializableMembers.First();
                    //     
                    //     using (writer.Scope($"if (propertyName == \"{singleMember.propertyName}\"))"))
                    //     {
                    //         writer.WriteLine($"value.{singleMember.memberName} = {singleMember.memberType.GetReadValueCode()};");
                    //     }
                    //     using (writer.Scope("else"))
                    //     {
                    //         writer.WriteLine("reader.Skip();");
                    //     }
                    // }
                    //
                    // if (serializableMembers.Length > 1)
                    // {
                    //     var firstMember = serializableMembers.First();
                    //     
                    //     using (writer.Scope($"if (propertyName == \"{firstMember.propertyName}\"))"))
                    //     {
                    //         writer.WriteLine($"value.{firstMember.memberName} = {firstMember.memberType.GetReadValueCode()};");
                    //     }
                    //
                    //     for (var i = 1; i < serializableMembers.Length; i++)
                    //     {
                    //         var serializableMember = serializableMembers[i];
                    //         
                    //         using (writer.Scope($"else if (propertyName == \"{serializableMember.propertyName}\"))"))
                    //         {
                    //             writer.WriteLine($"value.{serializableMember.memberName} = {serializableMember.memberType.GetReadValueCode()};");
                    //         }
                    //     }
                    //     
                    //     using (writer.Scope("else"))
                    //     {
                    //         writer.WriteLine("reader.Skip();");
                    //     }
                    // }
                    //
                    // writer.WriteLine("reader.Read();");
                    
                    using (writer.Scope("switch (propertyName)"))
                    {
                        foreach (var member in typeSymbol.GetSerializableMembers())
                        {
                            writer.WriteLine($"case \"{member.info.PropertyName}\":");
                            writer.WriteLine($"    value.{member.memberName} = {member.memberType.GetReadValueCode()};");
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