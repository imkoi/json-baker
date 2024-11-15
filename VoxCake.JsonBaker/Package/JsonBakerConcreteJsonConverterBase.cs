using Newtonsoft.Json;

namespace VoxCake.JsonBaker
{
    public abstract class JsonBakerConcreteJsonConverterBase : JsonConverter
    {
        public abstract void Initialize();
    }
}