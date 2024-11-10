using System;
using Newtonsoft.Json;

namespace VoxCake.JsonBaker
{
    public abstract class JsonBakerAssemblyConverterProviderBase
    {
        public abstract JsonConverter GetConverter(Type type);
    }
}