using System;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Carrot.Serialization
{
    public class JsonSerializer : ISerializer, ISerializer<string>//, IEncodedTextSerializer
    {
        public JsonSerializerSettings Settings { get; } = new JsonSerializerSettings();

        public Object Deserialize(Byte[] body, TypeInfo type, Encoding encoding = null)
        {
            var e = encoding ?? new UTF8Encoding(true);
            return JsonConvert.DeserializeObject(e.GetString(body),
                                                 type.AsType(),
                                                 Settings);
        }

        public string Serialize(object obj) => Serialize<object>(obj);

//        public byte[] Serialize(object obj, Encoding encoding)
//        {
//            var e = encoding ?? new UTF8Encoding(true);
//            return e.GetBytes(Serialize(obj));
//        }

        public T Deserialize<T>(string serialized)
        {
            return JsonConvert.DeserializeObject<T>(serialized, Settings);
        }

        public string Serialize<T>(T instance)
        {
            return JsonConvert.SerializeObject(instance, Settings);
        }
    }
}