using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace VoxCake.JsonBaker.SourceGenerator;

public class JsonPropertyUnsupportedAttributeException : Exception
{
    public KeyValuePair<string, TypedConstant>[] UnsupportedAttributes { get; }
    public DiagnosticDescriptor Diagnostic => new DiagnosticDescriptor(
        id: "JB001",
        title: "Unsupported NewtonsoftJson Attribute",
        messageFormat: "Property attribute '{0}' is not supported by JsonBaker",
        category: "JsonBaker",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public Location? Location { get; set; }

    public JsonPropertyUnsupportedAttributeException(Location? location, KeyValuePair<string, TypedConstant>[] unsupportedAttributes)
    {
        Location = location;
        UnsupportedAttributes = unsupportedAttributes;
    }
}