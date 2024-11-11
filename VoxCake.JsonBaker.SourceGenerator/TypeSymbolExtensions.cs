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

    private static readonly HashSet<string> _supportedJsonPropertyAttributeParameters = new HashSet<string>()
    {
        "PropertyName",
        "NullValueHandling"
    };
    
    public static IEnumerable<(string memberName, ITypeSymbol memberType, JsonPropertyInfo info)> GetSerializableMembers(this ITypeSymbol typeSymbol)
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
            
            var propertyInfo = member.GetJsonPropertyInfo();
            
            if (member is IPropertySymbol property && !property.IsStatic && property.SetMethod != null)
            {
                yield return (property.Name, property.Type, propertyInfo);
            }
            else if (member is IFieldSymbol field && !field.IsStatic)
            {
                yield return (field.Name, field.Type, propertyInfo);
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

    public static JsonPropertyInfo GetJsonPropertyInfo(this ISymbol member)
    {
        var propertyName = string.Empty;
        var includeNullValues = true;
        var includeDefaultValues = true;
            
        var propertyAttribute = member.GetAttributes().FirstOrDefault(attribute =>
            attribute.AttributeClass?.ToDisplayString() == "Newtonsoft.Json.JsonPropertyAttribute");

        if (propertyAttribute != null)
        {
            var unsupportedAttributes = propertyAttribute.NamedArguments
                .Where(arg => !_supportedJsonPropertyAttributeParameters.Contains(arg.Key))
                .ToArray();

            if (unsupportedAttributes.Length > 0)
            {
                throw new JsonPropertyUnsupportedAttributeException(propertyAttribute.ApplicationSyntaxReference.GetSyntax().GetLocation(), unsupportedAttributes);
            }
            
            var propertyNameAttribute = propertyAttribute.NamedArguments
                .Where(arg => arg.Key == "PropertyName").ToArray();

            if (propertyNameAttribute.Any())
            {
                var value = (string?) propertyNameAttribute.First().Value.Value;
                
                if (value != null)
                {
                    propertyName = value;
                }
            }
            else
            {
                propertyName = propertyAttribute.ConstructorArguments.FirstOrDefault().Value?.ToString();
            }

            var nullValueHandlingAttribute = propertyAttribute.NamedArguments
                .Where(arg => arg.Key == "NullValueHandling").ToArray();

            if (nullValueHandlingAttribute.Any())
            {
                var value = (int?) nullValueHandlingAttribute.First().Value.Value;

                if (value.HasValue)
                {
                    includeDefaultValues = value.Value == 0;
                }
            }
                
            var defaultValueHandlingAttribute = propertyAttribute.NamedArguments
                .Where(arg => arg.Key == "DefaultValueHandling").ToArray();
                
            if (defaultValueHandlingAttribute.Any())
            {
                var value = (int?) defaultValueHandlingAttribute.First().Value.Value;
                
                if (value.HasValue)
                {
                    includeDefaultValues = value.Value == 0;
                }
            }
        }
            
        if (string.IsNullOrEmpty(propertyName))
        {
            propertyName = member.Name;
        }

        return new JsonPropertyInfo(propertyName, includeNullValues, includeDefaultValues);
    }
}