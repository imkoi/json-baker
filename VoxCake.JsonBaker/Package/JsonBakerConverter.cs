using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace VoxCake.JsonBaker
{
    internal class JsonBakerConverter : JsonConverter
    {
        private readonly Action<string> _warningCallback;
        private readonly Dictionary<Type, JsonConverter> _converters = new Dictionary<Type, JsonConverter>(128);
        private readonly Dictionary<Assembly, JsonBakerAssemblyConverterProviderBase> _converterProviders = new Dictionary<Assembly, JsonBakerAssemblyConverterProviderBase>(16);
        private readonly HashSet<string> _excludedAssemblies = new HashSet<string>(StringComparer.Ordinal)
        {
            "System.Private.CoreLib",
            "System.Runtime"
        };

        public JsonBakerConverter(Action<string> warningCallback)
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
            var converter = default(JsonConverter);
            
            if (!_converters.TryGetValue(objectType, out converter))
            {
                var typeAssembly = objectType.Assembly;
                
                if (!_converterProviders.TryGetValue(typeAssembly, out var assemblyConverter))
                {
                    if (_excludedAssemblies.Contains(typeAssembly.GetName().Name))
                    {
#if !DEBUG
                        return false;
#endif
                    }
                
                    var converterType = typeAssembly.GetType("JsonBakerAssemblyConverterProvider", false, false);

                    if (converterType != null)
                    {
                        assemblyConverter = FormatterServices.GetUninitializedObject(converterType) as JsonBakerAssemblyConverterProviderBase;
                        
                        converter = assemblyConverter.GetConverter(objectType);
                    }
#if DEBUG
                    else
                    {
                        _warningCallback.Invoke($"'{objectType}' and all types inside '{typeAssembly.GetName().Name}' dont have baked converters, " +
                                                $"please exclude assembly from processing by passing it to {nameof(JsonBakerSettings)}.{nameof(JsonBakerSettings.ExcludeAssembly)}");
                    }
#endif
                
                    _converterProviders.Add(typeAssembly, assemblyConverter);
                }
                else
                {
                    if (assemblyConverter != null)
                    {
                        converter = assemblyConverter.GetConverter(objectType);
                    }
                }
                
                _converters.Add(objectType, converter);
            }
            
#if DEBUG
            var isConvertible = converter != null && converter.CanConvert(objectType);
            
            if (!isConvertible)
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