using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace VoxCake.JsonBaker
{
    internal class JsonBakerConverter : JsonConverter, IJsonBakerConverterResolver
    {
        private readonly Action<string> _warningCallback;
        private readonly Dictionary<Type, JsonConverter> _converters = new Dictionary<Type, JsonConverter>(128);
        private readonly Dictionary<Assembly, JsonBakerAssemblyConverterProviderBase> _converterProviders = new Dictionary<Assembly, JsonBakerAssemblyConverterProviderBase>(16);
        private readonly HashSet<string> _excludedAssemblies = new HashSet<string>(StringComparer.Ordinal)
        {
            "System.Private.CoreLib",
            "System.Runtime"
        };

        public JsonBakerConverter(Action<string> warningCallback = null)
        {
            _warningCallback = warningCallback;
        }

        public void ExcludeAssembly(string assemblyName)
        {
            _excludedAssemblies.Add(assemblyName);
        }
        
        public void ExcludeType(Type type)
        {
            _converters.Add(type, null);
        }

        public bool TryGetConcreteConverter(Type type, out JsonConverter converter)
        {
            CanConvert(type);
            
            converter = _converters[type];

            return converter != null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            _converters[value.GetType()].WriteJson(writer, value, serializer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return _converters[objectType].ReadJson(reader, objectType, existingValue, serializer);
        }

        public override bool CanConvert(Type objectType)
        {
            if (!_converters.TryGetValue(objectType, out var converter))
            {
                var typeAssembly = objectType.Assembly;
                
                if (!_converterProviders.TryGetValue(typeAssembly, out var converterProvider))
                {
                    if (!_excludedAssemblies.Contains(typeAssembly.GetName().Name))
                    {
                        var converterType = typeAssembly.GetType("JsonBakerAssemblyConverterProvider", false, false);

                        if (converterType != null)
                        {
                            converterProvider = FormatterServices.GetUninitializedObject(converterType) as JsonBakerAssemblyConverterProviderBase;
                        }
#if JSONBAKER_PUSH_PERFORMANCE_WARNINGS
                        else if(_warningCallback != null)
                        {
                            _warningCallback.Invoke($"'{objectType}' and all types inside '{typeAssembly.GetName().Name}' dont have baked converters, " +
                                                    $"please exclude assembly from processing by passing it to {nameof(JsonBakerSettings)}.{nameof(JsonBakerSettings.ExcludeAssembly)}");
                        }
#endif
                    }
                    
                    _converterProviders[typeAssembly] = converterProvider;
                    
                    converterProvider?.Initialize(this);
                }
                
                if (converterProvider != null)
                {
                    converter = converterProvider.GetConverter(objectType);
                }
                
                _converters[objectType] = converter;
            }
            
#if JSONBAKER_PUSH_PERFORMANCE_WARNINGS
            var isConvertible = converter != null && converter.CanConvert(objectType);
            
            if (!isConvertible && _warningCallback != null)
            {
                _warningCallback.Invoke($"'{objectType}' dont have baked converter, please exclude type from " +
                                        $"processing by passing it to {nameof(JsonBakerSettings)}.{nameof(JsonBakerSettings.ExcludeType)}");
            }

            return isConvertible;
#endif
            
            return converter != null && converter.CanConvert(objectType);
        }
    }
}