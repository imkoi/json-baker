using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace VoxCake.JsonBaker.SourceGenerator;

public static class JsonBakerConverterGenerator
{
    public static string GenerateAndGetConverterName(ITypeSymbol typeSymbol, CodeWriter writer)
    {
        var converterName = string.Empty;
        var resultConverterName = string.Empty;

        var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
        var namespaceScope = default(IDisposable);
        
        if (!string.IsNullOrEmpty(namespaceName))
        {
            resultConverterName += namespaceName + ".";

            namespaceScope = writer.Scope($"namespace {namespaceName}");
        }
        
        var containingType = typeSymbol.ContainingType;

        while (containingType != null)
        {
            var containingTypeName = containingType.Name + "_";
            
            converterName += containingTypeName;
            
            containingType = containingType.ContainingType;
        }
        
        var className = typeSymbol.Name;
        
        converterName += $"{className}Converter_Generated";
        resultConverterName += converterName;

        using (writer.Scope($"public class {converterName} : JsonConverter"))
        {
            using(writer.Scope("public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)"))
            {
                writer.WriteLine($"var concreteValue = ({typeSymbol.ToDisplayString()}) value;");
                
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
                
                writer.WriteLine($"var value = new {typeSymbol.ToDisplayString()}();");
                writer.WriteLine("reader.Read();");
                writer.EmptyLine();

                using (writer.Scope("while (reader.TokenType == JsonToken.PropertyName)"))
                {
                    writer.WriteLine("var propertyName = (string)reader.Value;");
                    writer.WriteLine("reader.Read();");
                    writer.WriteLine("");

                    var serializableMembers = typeSymbol.GetSerializableMembers().ToArray();

                    if (serializableMembers.Any())
                    {
                        if (serializableMembers.Length == 1)
                        {
                            var member = serializableMembers.First();

                            using (writer.Scope($"if (propertyName == \"{member.info.PropertyName}\")"))
                            {
                                writer.WriteLine($"value.{member.memberName} = {member.memberType.GetReadValueCode()};");
                            }
                            using (writer.Scope($"else if (propertyName.Equals(\"{member.info.PropertyName}\", StringComparison.OrdinalIgnoreCase))"))
                            {
                                writer.WriteLine($"value.{member.memberName} = {member.memberType.GetReadValueCode()};");
                            }
                            using (writer.Scope("else"))
                            {
                                writer.WriteLine("reader.Skip();");
                            }
                        }
                        else
                        {
                            var firstMember = serializableMembers.First();
                            
                            using (writer.Scope($"if (propertyName == \"{firstMember.info.PropertyName}\")"))
                            {
                                writer.WriteLine($"value.{firstMember.memberName} = {firstMember.memberType.GetReadValueCode()};");
                            }

                            for (var i = 1; i < serializableMembers.Length; i++)
                            {
                                var member = serializableMembers[i];
                                
                                using (writer.Scope($"else if (propertyName == \"{member.info.PropertyName}\")"))
                                {
                                    writer.WriteLine($"value.{member.memberName} = {member.memberType.GetReadValueCode()};");
                                }
                            }
      
                            foreach (var member in serializableMembers)
                            {
                                using (writer.Scope($"else if (propertyName.Equals(\"{member.info.PropertyName}\", StringComparison.OrdinalIgnoreCase))"))
                                {
                                    writer.WriteLine($"value.{member.memberName} = {member.memberType.GetReadValueCode()};");
                                }
                            }
                            
                            using (writer.Scope("else"))
                            {
                                writer.WriteLine("reader.Skip();");
                            }
                        }
                    }
                    else
                    {
                        writer.WriteLine("reader.Skip();");
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

        return resultConverterName;
    }
}