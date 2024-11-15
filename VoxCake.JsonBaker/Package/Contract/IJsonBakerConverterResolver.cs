using System;
using Newtonsoft.Json;

namespace VoxCake.JsonBaker
{
    public interface IJsonBakerConverterResolver
    {
        bool TryGetConcreteConverter(Type type, out JsonConverter converter);
    }
}