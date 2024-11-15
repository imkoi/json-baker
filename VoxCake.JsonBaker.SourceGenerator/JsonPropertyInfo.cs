using Microsoft.CodeAnalysis;

namespace VoxCake.JsonBaker.SourceGenerator;

public class JsonPropertyInfo
{
    public string PropertyName { get; }
    
    public bool IncludeNullValues { get; }
    public bool IncludeDefaultValues { get; }
    public bool IsCollection { get; }
    public ITypeSymbol? ElementType { get; }
    public bool HasBakeAttribute { get; }

    public JsonPropertyInfo(string propertyName, bool includeNullValues, bool includeDefaultValues, bool isCollection, ITypeSymbol elementType, bool hasBakeAttribute)
    {
        PropertyName = propertyName;
        IncludeNullValues = includeNullValues;
        IncludeDefaultValues = includeDefaultValues;
        IsCollection = isCollection;
        ElementType = elementType;
        HasBakeAttribute = hasBakeAttribute;
    }
}