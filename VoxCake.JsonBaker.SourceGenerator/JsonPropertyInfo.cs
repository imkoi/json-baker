using Microsoft.CodeAnalysis;

namespace VoxCake.JsonBaker.SourceGenerator;

public class JsonPropertyInfo
{
    public string PropertyName { get; }
    
    public bool IncludeNullValues { get; }
    public bool IsCollection { get; }
    public ITypeSymbol? ElementType { get; }
    public bool HasBakeAttribute { get; }
    public JsonDefaultValueHandling? DefaultValueHandling { get; }

    public JsonPropertyInfo(string propertyName, bool includeNullValues, bool isCollection, ITypeSymbol elementType, bool hasBakeAttribute, JsonDefaultValueHandling? defaultValueHandling)
    {
        PropertyName = propertyName;
        IncludeNullValues = includeNullValues;
        IsCollection = isCollection;
        ElementType = elementType;
        HasBakeAttribute = hasBakeAttribute;
        DefaultValueHandling = defaultValueHandling;
    }
}