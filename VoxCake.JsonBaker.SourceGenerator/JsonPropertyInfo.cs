namespace VoxCake.JsonBaker.SourceGenerator;

public class JsonPropertyInfo
{
    public string PropertyName { get; }
    
    public bool IncludeNullValues { get; }
    public bool IncludeDefaultValues { get; }

    public JsonPropertyInfo(string propertyName, bool includeNullValues, bool includeDefaultValues)
    {
        PropertyName = propertyName;
        IncludeNullValues = includeNullValues;
        IncludeDefaultValues = includeDefaultValues;
    }
}