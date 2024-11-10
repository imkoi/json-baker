using System.Collections.Generic;
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
    
    public static IEnumerable<(string name, ITypeSymbol type)> GetSerializableMembers(this ITypeSymbol typeSymbol)
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