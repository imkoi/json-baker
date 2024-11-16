namespace VoxCake.JsonBaker.SourceGenerator;

public class JsonDefaultValueHandling
{
    public enum HandlingType
    {
        Include = 0,
        Ignore = 1,
        Populate = 2,
        IgnoreAndPopulate = Ignore | Populate
    }
    
    public HandlingType Type { get; }
    public string Value { get; }

    public JsonDefaultValueHandling(HandlingType type, string value)
    {
        Type = type;
        Value = value;
    }
}