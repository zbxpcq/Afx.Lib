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
        public int Status
        {
            get { return this.status; }
            set { this.status = value; }
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
        /// <param name="model">消息model</param>
        public void SetData(object model)
        {
            this.Data = SerializerUtils.Serialize(model);
        }

        /// <summary>
        /// 获取消息数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetData<T>()
        {
            return SerializerUtils.Deserialize<T>(this.Data);
        }
        /// <summary>
        /// 获取消息数据
        /// </summary>
        /// <param name="type">消息model type</param>
        /// <returns>消息model</returns>
        public object GetData(Type type)
        {
            return SerializerUtils.Deserialize(type, this.Data);
        }

        /// <summary>
        /// 清除状态、消息数据、请求返回消息
        /// </summary>
        public void Rest()
        {
            this.Status = (int)MsgStatus.None;
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
            return SerializerUtils.Deserialize<MsgData>(buffer);
        }

        /// <summary>
        /// 序列化消息
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            return SerializerUtils.Serialize(this);
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
