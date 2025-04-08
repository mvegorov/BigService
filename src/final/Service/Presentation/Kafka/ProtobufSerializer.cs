using Confluent.Kafka;
using Newtonsoft.Json;
using System.Text;

namespace Presentation.Kafka;

public class ProtobufSerializer<T> : ISerializer<T>, IDeserializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        return Encoding.Default.GetBytes(JsonConvert.SerializeObject(data));
    }

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull)
        {
            string message = $"Error deserializing protobuf message of Type = {typeof(T)}, null value found";
            throw new ArgumentNullException(nameof(data), message);
        }

        string serialized = Encoding.Default.GetString(data);
        T? value = JsonConvert.DeserializeObject<T>(serialized);

        if (value is null)
        {
            string message = $"Error deserializing protobuf message of Type = {typeof(T)}, null value found";
            throw new ArgumentNullException(nameof(data), message);
        }

        return value;
    }
}