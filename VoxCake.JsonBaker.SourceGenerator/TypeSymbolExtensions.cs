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
    
    public static string GetReadValueCode(this JsonSerializableMemberInfo member)
    {
        var value = string.Empty;
        
        var memberType = member.MemberType;
        
        if (memberType.IsPrimitiveType())
        {
            var typeName = memberType.ToDisplayString();

            switch (typeName)
            {
                case "string":
                case "string?":
                    value = "(string)reader.Value";
                    break;
                case "int":
                case "int?":
                    value = "reader.Value != null ? Convert.ToInt32(reader.Value) : default";
                    break;
                case "long":
                case "long?":
                    value = "reader.Value != null ? Convert.ToInt64(reader.Value) : default";
                    break;
                case "float":
                case "float?":
                    value = "reader.Value != null ? Convert.ToSingle(reader.Value) : default";
                    break;
                case "double":
                case "double?":
                    value = "reader.Value != null ? Convert.ToDouble(reader.Value) : default";
                    break;
                case "bool":
                case "bool?":
                    value = "reader.Value != null ? Convert.ToBoolean(reader.Value) : default";
                    break;
                case "DateTime":
                case "DateTime?":
                    value = "reader.Value != null ? Convert.ToDateTime(reader.Value) : default";
                    break;
                default:
                    value = $"({typeName})reader.Value";
                    break;
            }
        }
        else if (member.Info.HasBakeAttribute)
        {
            var castType = member.Info.ElementType ?? member.MemberType;
                                    
            value = $"({castType.ToDisplayString()})" + member.MemberName + $"_JsonConverter.ReadJson(reader, typeof({castType.ToDisplayString()}), existingValue, serializer)";
        }
        else 
        {
            value = $"serializer.Deserialize<{memberType.ToDisplayString()}>(reader)";
        }

        return value;
    }

    public static bool IsPrimitiveType(this ITypeSymbol typeSymbol)
    {
        var displayString = typeSymbol.ToDisplayString();

        return _primitiveTypes.Contains(displayString);
    }

    public static JsonPropertyInfo? GetJsonPropertyInfo(this ISymbol member)
    {
        var propertyName = string.Empty;
        var includeNullValues = true;
        var includeDefaultValues = true;

        var memberType = default(ITypeSymbol);

        if (member is IPropertySymbol property)
        {
            memberType = property.Type;
        }

        if (member is IFieldSymbol field)
        {
            memberType = field.Type;
        }

        if (memberType == null)
        {
            return null;
        }

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

        var isBaked = memberType.GetAttributes().Any(attr =>
            attr.AttributeClass?.ToDisplayString() == "VoxCake.JsonBaker.JsonBakerAttribute");
        
        var ienumerableType = memberType.AllInterfaces.FirstOrDefault(t => t.ToDisplayString().Contains("ICollection"));
        var elementType = default(ITypeSymbol);
                    
        if (ienumerableType != null)
        {
            var collectionTypeSymbol = (INamedTypeSymbol) memberType;
            elementType = collectionTypeSymbol.TypeArguments.FirstOrDefault();
            
            isBaked = elementType.GetAttributes().Any(attr =>
                attr.AttributeClass?.ToDisplayString() == "VoxCake.JsonBaker.JsonBakerAttribute");
        }

        return new JsonPropertyInfo(propertyName, includeNullValues, includeDefaultValues, ienumerableType != null, elementType, isBaked);
    }
}