using Microsoft.CodeAnalysis;

namespace VoxCake.JsonBaker.SourceGenerator;

public class JsonSerializableMemberInfo
{
    public string MemberName { get; }
    public ITypeSymbol MemberType { get; }
    public JsonPropertyInfo Info { get; }

    public JsonSerializableMemberInfo(string memberName, ITypeSymbol memberType, JsonPropertyInfo info)
    {
        MemberName = memberName;
        MemberType = memberType;
        Info = info;
    }
}