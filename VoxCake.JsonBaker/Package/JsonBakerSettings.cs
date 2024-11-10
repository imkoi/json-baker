using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VoxCake.JsonBaker
{
    public class JsonBakerSettings
    {
        public static JsonSerializerSettings Default => GetDefaultSettings();
        
        public static event Action<string> WarningReceived;

        private static JsonSerializerSettings _settings;
#if JSONBAKER_PUSH_PERFORMANCE_WARNINGS
        private static JsonBakerConverter _converter = new JsonBakerConverter(OnWarning);
#else
        private static JsonBakerConverter _converter = new JsonBakerConverter();
#endif
        private static DefaultContractResolver _contractResolver = new DefaultContractResolver();
        
        public static void ExcludeAssembly(string assemblyName)
        {
            _converter.ExcludeAssembly(assemblyName);
        }
        
        public static void ExcludeType(Type type)
        {
            _converter.ExcludeType(type);
        }

        private static JsonSerializerSettings GetDefaultSettings()
        {
            if (_settings != null)
            {
                return _settings;
            }
            
            _converter = new JsonBakerConverter(OnWarning);
            _contractResolver = new DefaultContractResolver();
            
            _settings = new()
            {
                Converters = new List<JsonConverter>(8){ _converter },
                ContractResolver = _contractResolver,
            };

            return _settings;
        }
        
        private static void OnWarning(string message)
        {
            WarningReceived?.Invoke(message);
        }
    }
}