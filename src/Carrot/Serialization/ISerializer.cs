using System;
using System.Reflection;
using System.Text;

namespace Carrot.Serialization
{
    public interface ISerializer
    {
        Object Deserialize(Byte[] body, TypeInfo type, Encoding encoding = null);

        String Serialize(Object obj);
    }

    public interface ISerializer<T>
    {
        TTo Deserialize<TTo>(T serialized);

        T Serialize<TFrom>(TFrom instance);
    }
}