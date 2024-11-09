using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VoxCake.JsonBaker
{
    public class JsonBakerAssemblyConverter : JsonConverter
    {
        private Dictionary<Type, JsonConverter> _converters;
        
        private bool _initialized;
        
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
            if (!_initialized)
            {
                Initialize();
                _initialized = true;
            }
            
            return _converters.TryGetValue(objectType, out JsonConverter converter) && converter.CanConvert(objectType);
        }

        private void Initialize()
        {
            _converters = new Dictionary<Type, JsonConverter>(16)
            {

            };
        }
    }
}