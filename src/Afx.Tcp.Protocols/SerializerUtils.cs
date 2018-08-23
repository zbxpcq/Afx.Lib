using System;
using System.Collections.Generic;
using System.IO;

using ProtoBuf;

namespace Afx.Tcp.Protocols
{
    /// <summary>
    /// SerializerUtils
    /// </summary>
    public static class SerializerUtils
    {
        /// <summary>
        /// IsProtoBuf
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsProtoBuf<T>()
        {
            return IsProtoBuf(typeof(T));
        }

        /// <summary>
        /// IsProtoBuf
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsProtoBuf(Type type)
        {
            if(type.IsArray)
            {
                type = type.GetElementType();
            }

            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                type = type.GetGenericArguments()[0];
            }
            var att = Attribute.GetCustomAttributes(type, typeof(ProtoContractAttribute), false);
            return att != null && att.Length > 0;
        }

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] buffer)
        {
            T model = default(T);
            var obj = Deserialize(model != null ? model.GetType() : typeof(T), buffer);
            if (obj != null)
            {
                model = (T)obj;
            }

            return model;
        }

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="type"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static object Deserialize(Type type, byte[] buffer)
        {
            object model = null;
            if (type == null) throw new ArgumentNullException("type");
            if (type == typeof(byte[]))
            {
                model = buffer;
            }
            else
            {
                if (buffer != null && buffer.Length > 0)
                {
                    using (var ms = new MemoryStream(buffer))
                    {
                        if (IsProtoBuf(type))
                        {
                            model = Serializer.Deserialize(type, ms);
                        }
                        else
                        {
                            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                            model = formatter.Deserialize(ms);
                        }
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static byte[] Serialize(object model)
        {
            byte[] buffer = null;
            if (model != null)
            {
                var type = model.GetType();
                if (type == typeof(byte[]))
                {
                    buffer = (byte[])((object)model);
                }
                else
                {
                    using (var ms = new MemoryStream())
                    {
                        if (IsProtoBuf(type))
                        {
                            Serializer.Serialize(ms, model);
                        }
                        else
                        {
                            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                            formatter.Serialize(ms, model);
                        }

                        buffer = ms.ToArray();
                    }
                }
            }

            return buffer;
        }
    }
}
