using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace VoxCake.JsonBaker.SourceGenerator;

public static class JsonBakerConverterGenerator
{
    private static readonly string[] ConstructorParameters =
    {
        "VoxCake.JsonBaker.IJsonBakerConverterResolver converterResolver"
    };
    
    private static readonly string[] ConstructorLines =
    {
        "_converterResolver = converterResolver;"
    };
    
    public static string GenerateAndGetConverterName(ITypeSymbol typeSymbol, CodeWriter writer)
    {
        var resultConverterName = string.Empty;

        using (writer.CreateClass(typeSymbol, ConstructorParameters, ConstructorLines, out resultConverterName, out var serializableMembers))
        {
            var bakedMembers = serializableMembers
                .Where(m => m.Info.HasBakeAttribute)
                .ToArray();
            
            writer.WriteLine("private VoxCake.JsonBaker.IJsonBakerConverterResolver _converterResolver;");

            foreach (var bakedMember in bakedMembers)
            {
                writer.WriteLine($"private Newtonsoft.Json.JsonConverter {bakedMember.MemberName}_JsonConverter;");
            }

            if (bakedMembers.Any())
            {
                writer.WriteEmptyLine();
            }

            using (writer.Scope("public override void Initialize()"))
            {
                foreach (var bakedMember in bakedMembers)
                {
                    var memberTypeName = bakedMember.Info.ElementType?.ToDisplayString() ??
                                            bakedMember.MemberType.ToDisplayString();
                    
                    writer.WriteLine($"_converterResolver.TryGetConcreteConverter(typeof({memberTypeName}), out {bakedMember.MemberName}_JsonConverter);");
                }
            }
            
            using(writer.Scope("public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)"))
            {
                writer.WriteLine($"var concreteValue = ({typeSymbol.ToDisplayString()}) value;");
                
                writer.WriteLine("writer.WriteStartObject();");
                writer.WriteEmptyLine();

                using (writer.Scope("// Write all serializable properties"))
                {
                    var memberIndex = 0;
                    
                    foreach (var member in serializableMembers)
                    {
                        if (memberIndex != 0)
                        {
                            writer.WriteEmptyLine();    
                        }
                        
                        writer.WriteLine($"// Write '{member.MemberName}' property");
                        
                        var targetVariableName = $"concreteValue.{member.MemberName}";
                        IDisposable defaultCheckScope = new EmptyScope();

                        if (!member.Info.IncludeDefaultValues || !member.Info.IncludeNullValues)
                        {
                            defaultCheckScope = writer.Scope($"if (concreteValue.{member.MemberName} != default)");
                        }

                        using (defaultCheckScope)
                        {
                            writer.WriteLine($"writer.WritePropertyName(\"{member.Info.PropertyName}\");");

                            IDisposable foreachScope = new EmptyScope();

                            if (member.Info.IsCollection)
                            {
                                writer.WriteLine("writer.WriteStartArray();");

                                foreachScope = writer.Scope(
                                        $"foreach (var element in concreteValue.{member.MemberName})",
                                        placeEmptyLineAfterScope: false);
                                targetVariableName = "element";
                            }

                            using (foreachScope)
                            {
                                if (member.MemberType.IsPrimitiveType())
                                {
                                    writer.WriteLine($"writer.WriteValue({targetVariableName});");
                                }
                                else if (member.Info.HasBakeAttribute)
                                {
                                    writer.WriteLine(
                                        $"{member.MemberName}_JsonConverter.WriteJson(writer, {targetVariableName}, serializer);");
                                }
                                else
                                {
                                    writer.WriteLine($"serializer.Serialize(writer, {targetVariableName});");
                                }
                            }

                            if (member.Info.IsCollection)
                            {
                                writer.WriteLine("writer.WriteEndArray();");
                            }
                        }

                        memberIndex++;
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
                writer.WriteEmptyLine();

                using (writer.Scope("while (reader.TokenType == JsonToken.PropertyName)"))
                {
                    writer.WriteLine("var propertyName = (string)reader.Value;");
                    writer.WriteLine("reader.Read();");
                    writer.WriteLine("");
                    
                    if (serializableMembers.Any())
                    {
                        if (serializableMembers.Length == 1)
                        {
                            var member = serializableMembers.First();

                            using (writer.Scope($"if (propertyName == \"{member.Info.PropertyName}\")", placeEmptyLineAfterScope: false))
                            {
                                WriteReadProperty(writer, member);
                            }
                            using (writer.Scope($"else if (propertyName.Equals(\"{member.Info.PropertyName}\", StringComparison.OrdinalIgnoreCase))", placeEmptyLineAfterScope: false))
                            {
                                WriteReadProperty(writer, member);
                            }
                            using (writer.Scope("else"))
                            {
                                writer.WriteLine("reader.Skip();");
                            }
                        }
                        else
                        {
                            var firstMember = serializableMembers.First();

                            using (writer.Scope($"if (propertyName == \"{firstMember.Info.PropertyName}\")", placeEmptyLineAfterScope: false))
                            {
                                WriteReadProperty(writer, firstMember);
                            }

                            for (var i = 1; i < serializableMembers.Length; i++)
                            {
                                using (writer.Scope($"else if (propertyName == \"{serializableMembers[i].Info.PropertyName}\")", placeEmptyLineAfterScope: false))
                                {
                                    WriteReadProperty(writer, serializableMembers[i]);
                                }
                            }
      
                            foreach (var member in serializableMembers)
                            {
                                using (writer.Scope($"else if (propertyName.Equals(\"{member.Info.PropertyName}\", StringComparison.OrdinalIgnoreCase))", placeEmptyLineAfterScope: false))
                                {
                                    WriteReadProperty(writer, member);
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

        return resultConverterName;
    }

    private static IDisposable CreateClass(this CodeWriter writer, ITypeSymbol typeSymbol,
        string[] constructorParameters, string[] constructorLines,
        out string resultConverterName, out JsonSerializableMemberInfo[] serializableMembers)
    {
        var converterName = string.Empty;
        resultConverterName = string.Empty;
        
        var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
        IDisposable namespaceScope = new EmptyScope();
        
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

        var innnerserializableMembers = typeSymbol.GetSerializableMembers().ToArray();
        serializableMembers = innnerserializableMembers
            .Select(m => new JsonSerializableMemberInfo(m.memberName, m.memberType, m.info))
            .ToArray();

        var classScope = writer.Scope($"public class {converterName} : JsonBakerConcreteJsonConverterBase");
        
        using (writer.Scope($"public {converterName}({string.Join(",", constructorParameters)})"))
        {
            foreach (var constructorLine in constructorLines)
            {
                writer.WriteLine(constructorLine);
            }
        }

        return new CompositeScope(namespaceScope, classScope);
    }

    private static void WriteReadProperty(CodeWriter writer, JsonSerializableMemberInfo member)
    {
        var value = member.GetReadValueCode();
        
        if (member.Info.HasBakeAttribute && member.Info.IsCollection)
        {
            var getElementCode = value;
            value = "collection";
            
            writer.WriteLine($"var collection = new {member.MemberType.ToDisplayString()}();");

            using (writer.Scope("if (reader.TokenType == JsonToken.StartArray)"))
            {
                writer.WriteLine("reader.Read();");

                using (writer.Scope("while(reader.TokenType == JsonToken.StartObject)"))
                {
                    writer.WriteLine($"var element = {getElementCode};");

                    writer.WriteLine("reader.Read();");
                    
                    writer.WriteLine("collection.Add(element);");
                }
            }
        }
        
        writer.WriteLine($"value.{member.MemberName} = {value};");
    }
}