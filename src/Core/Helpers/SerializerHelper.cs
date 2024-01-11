using System.Text;
using System.Text.Json;

namespace Core.Helpers
{
    public interface ISerializerHelper
    {
        byte[] Serialize<T>(T value);

        string SerializeObject(object obj);

        T Deserialize<T>(byte[] bytes);

        T DeserializeObject<T>(string value);

        T DeepCopy<T>(T other);
    }

    public class SerializerHelper : ISerializerHelper
    {
        public byte[] Serialize<T>(T value)
        {
            var str = JsonSerializer.Serialize(value);

            return Encoding.UTF8.GetBytes(str);
        }

        public string SerializeObject(object obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            var str = Encoding.UTF8.GetString(bytes);

            return DeserializeObject<T>(str);
        }

        public T DeserializeObject<T>(string value)
        {
            return JsonSerializer.Deserialize<T>(value);
        }

        public T DeepCopy<T>(T other)
        {
            if (other == null) return default;

            var bytes = Serialize(other);

            return Deserialize<T>(bytes);
        }
    }
}