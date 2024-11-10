using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace VoxCake.JsonBaker.SourceGenerator;

public static class TypeSymbolExtensions
{
    private static readonly HashSet<string> _primitiveTypes = new HashSet<string>
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
    
    public static IEnumerable<(string memberName, ITypeSymbol memberType, string propertyName)> GetSerializableMembers(this ITypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member.Name.EndsWith(">k__BackingField"))
            {
                continue;
            }
            
            if (member.GetAttributes().Any(attribute =>
                    attribute.AttributeClass?.ToDisplayString() == "Newtonsoft.Json.JsonIgnoreAttribute"))
            {
                continue;
            }
            
            var propertyName = string.Empty;
            
            var propertyAttribute = member.GetAttributes().FirstOrDefault(attribute =>
                attribute.AttributeClass?.ToDisplayString() == "Newtonsoft.Json.JsonPropertyAttribute");

            if (propertyAttribute != null)
            {
                propertyName = propertyAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "PropertyName")
                    .Value.Value?.ToString();
                
                if (string.IsNullOrEmpty(propertyName))
                {
                    propertyName = propertyAttribute.ConstructorArguments.FirstOrDefault().Value?.ToString();
                }
            }
            
            if (string.IsNullOrEmpty(propertyName))
            {
                propertyName = member.Name;
            }
            
            if (member is IPropertySymbol property && !property.IsStatic && property.SetMethod != null)
            {
                yield return (property.Name, property.Type, propertyName);
            }
            else if (member is IFieldSymbol field && !field.IsStatic)
            {
                yield return (field.Name, field.Type, propertyName);
            }
        }
    }
    
    public static string GetReadValueCode(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.IsPrimitiveType())
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

    public static bool IsPrimitiveType(this ITypeSymbol typeSymbol)
    {
        var displayString = typeSymbol.ToDisplayString();

        return _primitiveTypes.Contains(displayString);
    }
}