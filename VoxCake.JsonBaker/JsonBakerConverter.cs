using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace VoxCake.JsonBaker
{
    internal class JsonBakerConverter : JsonConverter
    {
        private Dictionary<Assembly, JsonConverter> _assemblyConverters = new Dictionary<Assembly, JsonConverter>(16);
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var assemblyConverter = _assemblyConverters[value.GetType().Assembly];
            
            assemblyConverter.WriteJson(writer, value, serializer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var assemblyConverter = _assemblyConverters[objectType.Assembly];
            
            return assemblyConverter.ReadJson(reader, objectType, existingValue, serializer);
        }

        public override bool CanConvert(Type objectType)
        {
            var typeAssembly = objectType.Assembly;
            
            if (!_assemblyConverters.TryGetValue(typeAssembly, out var assemblyConverter))
            {
                var converterType = typeAssembly.GetType("JsonBakerAssemblyConverter", false, false);

                if (converterType != null)
                {
                    assemblyConverter = FormatterServices.GetUninitializedObject(converterType) as JsonConverter;
                }
                
                _assemblyConverters.Add(typeAssembly, assemblyConverter);
            }
            
            var isConvertible = assemblyConverter != null && assemblyConverter.CanConvert(objectType);
            
            return isConvertible;
        }
    }
}