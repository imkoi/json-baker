using System;
using Newtonsoft.Json.Serialization;

namespace VoxCake.JsonBaker
{
    public class JsonBakerContractResolver : IContractResolver
    {
        private readonly DefaultContractResolver _defaultContractResolver;
        
        public JsonBakerContractResolver()
        {
            
        }
        
        public JsonContract ResolveContract(Type type)
        {
            return new JsonObjectContract(type);
        }
    }
}