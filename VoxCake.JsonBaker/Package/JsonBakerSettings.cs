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

        private static readonly JsonBakerConverter Converter = new JsonBakerConverter();
        private static readonly DefaultContractResolver ContractResolver = new DefaultContractResolver();
    }
}