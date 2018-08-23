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

//        public byte[] Serialize(object obj, Encoding encoding)
//        {
//            var e = encoding ?? new UTF8Encoding(true);
//            return e.GetBytes(Serialize(obj));
//        }

        public Object Deserialize(String body, Type type)
        {
            return JsonConvert.DeserializeObject(body, type, Settings);
        }

        public String Serialize(Object obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }
    }
}