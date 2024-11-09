using Newtonsoft.Json;

namespace VoxCake.JsonBaker
{
    public class JsonBakerSettings
    {
        public static JsonSerializerSettings Default => new() { Converters = { Converter } };

        private static readonly JsonBakerConverter Converter = new JsonBakerConverter();
    }
}