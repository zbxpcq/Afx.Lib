using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;
using System.IO;

namespace Afx.Tcp.Protocols
{
    /// <summary>
    /// 消息数据
    /// </summary>
    [ProtoContract]
    public class MsgData : IDisposable
    {
        [ProtoMember(1, IsRequired = true)]
        private int cmd;
        [ProtoMember(2)]
        private int id;
        [ProtoMember(3)]
        private int status;
        [ProtoMember(4)]
        private string msg;
        [ProtoMember(5)]
        private int length;
        [ProtoMember(6)]
        private byte[] data;

        /// <summary>
        /// 消息代码
        /// </summary>
        public int Cmd
        {
            get { return this.cmd; }
            set { this.cmd = value; }
        }

        /// <summary>
        /// 客户端请求id
        /// </summary>
        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        /// <summary>
        /// 请求状态
        /// </summary>
        public MsgStatus Status
        {
            get { return (MsgStatus)this.status; }
            set { this.status = (int)value; }
        }

        /// <summary>
        /// 请求返回消息
        /// </summary>
        public string Msg
        {
            get { return this.msg; }
            set { this.msg = value; }
        }

        /// <summary>
        /// 数据长度
        /// </summary>
        public int Length
        {
            get { return this.length; }
        }

        /// <summary>
        /// 消息数据
        /// </summary>
        public byte[] Data
        {
            get { return this.data; }
            private set 
            { 
                this.data = value;
                if (this.data == null) this.length = 0;
                else this.length = this.data.Length;
            }
        }

        /// <summary>
        /// 设置消息数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">消息model</param>
        public void SetData<T>(T model)
        {
            this.Data = Serialize(model);
        }

        /// <summary>
        /// 获取消息数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetData<T>()
        {
            return Deserialize<T>(this.Data);
        }

        /// <summary>
        /// 清除状态、消息数据、请求返回消息
        /// </summary>
        public void Rest()
        {
            this.Status = MsgStatus.None;
            this.Msg = null;
            //this.Data = null;
        }

        /// <summary>
        /// 反序列化消息
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static MsgData Deserialize(byte[] buffer)
        {
            return Deserialize<MsgData>(buffer);
        }

        /// <summary>
        /// 序列化消息
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            return Serialize(this);
        }

        private static T Deserialize<T>(byte[] buffer)
        {
            T model = default(T);
            if (typeof(T) == typeof(byte[]))
            {
                model = (T)((object)buffer);
            }
            else
            {
                if (buffer != null && buffer.Length > 0)
                {
                    try
                    {
                        using (var ms = new MemoryStream(buffer))
                        {
                           var att = Attribute.GetCustomAttribute(typeof(T), typeof(ProtoContractAttribute), false);
                            if (att != null)
                            {
                                model = Serializer.Deserialize<T>(ms);
                            }
                            else
                            {
                               var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                                model = (T)formatter.Deserialize(ms);
                            }
                        }
                    }
                    catch { }
                }
            }

            return model;
        }

        private static byte[] Serialize<T>(T model)
        {
            byte[] buffer = null;
            if (model != null)
            {
                if (typeof(T) == typeof(byte[]))
                {
                    buffer = (byte[])((object)model);
                }
                else
                {
                    try
                    {
                        using (var ms = new MemoryStream())
                        {
                            var att = Attribute.GetCustomAttribute(typeof(T), typeof(ProtoContractAttribute), false);
                            if (att != null)
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
                    catch { }
                }
            }

            return buffer;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            this.msg = null;
            this.data = null;
        }
    }
}
