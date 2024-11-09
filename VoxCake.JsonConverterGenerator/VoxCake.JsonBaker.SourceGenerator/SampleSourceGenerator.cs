using System;
using System.Collections.Generic;
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

        foreach (var typeSymbol in receiver.SerializableTypes)
        {
            var source = GenerateConverter(typeSymbol);
            context.AddSource($"{typeSymbol.Name}Converter_Generated.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    private string GenerateConverter(INamedTypeSymbol typeSymbol)
    {
        var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
        var className = typeSymbol.Name;
        var converterName = $"{className}Converter_Generated";

        var sb = new StringBuilder();

        sb.AppendLine("using System;");
        sb.AppendLine("using Newtonsoft.Json;");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(namespaceName))
        {
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
        }

        sb.AppendLine($"    public class {converterName} : JsonConverter<{className}>");
        sb.AppendLine("    {");
        sb.AppendLine($"        public override void WriteJson(JsonWriter writer, {className} value, JsonSerializer serializer)");
        sb.AppendLine("        {");
        sb.AppendLine("            writer.WriteStartObject();");

        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IPropertySymbol property && !property.IsStatic && property.GetMethod != null)
            {
                var propertyName = property.Name;

                sb.AppendLine($"            writer.WritePropertyName(nameof(value.{propertyName}));");

                if (IsPrimitiveType(property.Type))
                {
                    sb.AppendLine($"            writer.WriteValue(value.{propertyName});");
                }
                else
                {
                    sb.AppendLine($"            serializer.Serialize(writer, value.{propertyName});");
                }
            }
            else if (member is IFieldSymbol field && !field.IsStatic)
            {
                var fieldName = field.Name;

                if (!fieldName.EndsWith(">k__BackingField"))
                {
                    sb.AppendLine($"            writer.WritePropertyName(nameof(value.{fieldName}));");

                    if (IsPrimitiveType(field.Type))
                    {
                        sb.AppendLine($"            writer.WriteValue(value.{fieldName});");
                    }
                    else
                    {
                        sb.AppendLine($"            serializer.Serialize(writer, value.{fieldName});");
                    }
                }
            }
        }

        sb.AppendLine("            writer.WriteEndObject();");
        sb.AppendLine("        }");
        sb.AppendLine();

        sb.AppendLine($"        public override {className} ReadJson(JsonReader reader, Type objectType, {className} existingValue, bool hasExistingValue, JsonSerializer serializer)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (reader.TokenType == JsonToken.Null)");
        sb.AppendLine("                return null;");
        sb.AppendLine();
        sb.AppendLine($"            var value = new {className}();");
        sb.AppendLine("            reader.Read();");
        sb.AppendLine();
        sb.AppendLine("            while (reader.TokenType == JsonToken.PropertyName)");
        sb.AppendLine("            {");
        sb.AppendLine("                var propertyName = (string)reader.Value;");
        sb.AppendLine("                reader.Read();");
        sb.AppendLine();
        sb.AppendLine("                switch (propertyName)");
        sb.AppendLine("                {");

        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IPropertySymbol property && !property.IsStatic && property.SetMethod != null)
            {
                var propertyName = property.Name;
                var readValueCode = GetReadValueCode(property.Type);

                sb.AppendLine($"                    case nameof(value.{propertyName}):");
                sb.AppendLine($"                        value.{propertyName} = {readValueCode};");
                sb.AppendLine("                        break;");
            }
            else if (member is IFieldSymbol field && !field.IsStatic)
            {
                var fieldName = field.Name;

                if (!fieldName.EndsWith(">k__BackingField"))
                {
                    var readValueCode = GetReadValueCode(field.Type);

                    sb.AppendLine($"                    case nameof(value.{fieldName}):");
                    sb.AppendLine($"                        value.{fieldName} = {readValueCode};");
                    sb.AppendLine("                        break;");
                }
            }
        }

        sb.AppendLine("                    default:");
        sb.AppendLine("                        reader.Skip();");
        sb.AppendLine("                        break;");
        sb.AppendLine("                }");
        sb.AppendLine();
        sb.AppendLine("                reader.Read();");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            return value;");
        sb.AppendLine("        }");

        sb.AppendLine("    }");

        if (!string.IsNullOrEmpty(namespaceName))
        {
            sb.AppendLine("}");
        }

        return sb.ToString();
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
