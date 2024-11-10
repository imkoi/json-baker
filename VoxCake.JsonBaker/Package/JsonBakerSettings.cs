using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VoxCake.JsonBaker
{
    public class JsonBakerSettings
    {
        public static JsonSerializerSettings Default => new()
        {
            Converters = { Converter },
            ContractResolver = ContractResolver,
        };
        
        public static event Action<string> WarningReceived;

        private static readonly JsonBakerConverter Converter = new JsonBakerConverter(OnWarning);
        private static readonly DefaultContractResolver ContractResolver = new DefaultContractResolver();

        public static void ExcludeAssembly(string assemblyName)
        {
            Converter.ExcludeAssembly(assemblyName);
        }
        
        public static void ExcludeType(Type type)
        {
            Converter.ExcludeType(type);
        }
        
        private static void OnWarning(string message)
        {
            WarningReceived?.Invoke(message);
        }
    }
}