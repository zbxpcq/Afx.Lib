using System;
using System.Collections.Generic;
using System.Text;

using Afx.Tcp.Protocols;

namespace  Afx.Tcp.Host
{
    /// <summary>
    /// ActionResult
    /// </summary>
    public class ActionResult : IDisposable
    {
        /// <summary>
        /// Result
        /// </summary>
        public MsgData Result { get; private set; }

        /// <summary>
        /// ActionResult
        /// </summary>
        public ActionResult()
        {
            this.Result = new MsgData();
        }
        /// <summary>
        /// ActionResult
        /// </summary>
        /// <param name="msg"></param>
        public ActionResult(MsgData msg)
        {
            if (msg == null) throw new ArgumentNullException("msg");
            this.Result = msg;
        }

        /// <summary>
        /// Set send msg data
        /// </summary>
        /// <typeparam name="T">protobuf model</typeparam>
        /// <param name="status"></param>
        /// <param name="data"></param>
        /// <param name="msg"></param>
        public virtual void SetMsg(int status, string msg, object data)
        {
            this.Result.Rest();
            this.Result.Status = status;
            this.Result.SetData(data);
            this.Result.Msg = msg;
        }


        /// <summary>
        /// Set send msg data
        /// </summary>
        /// <typeparam name="T">protobuf model</typeparam>
        /// <param name="status"></param>
        /// <param name="data"></param>
        /// <param name="msg"></param>
        public virtual void SetMsg(MsgStatus status, string msg, object data)
        {
            this.Result.Rest();
            this.Result.Status = (int)status;
            this.Result.SetData(data);
            this.Result.Msg = msg;
        }

        /// <summary>
        /// Set send msg data
        /// </summary>
        /// <param name="status"></param>
        /// <param name="msg"></param>
        public virtual void SetMsg(int status, string msg)
        {
            this.Result.Rest();
            this.Result.Status = status;
            this.Result.Msg = msg;
        }


        /// <summary>
        /// Set send msg data
        /// </summary>
        /// <param name="status"></param>
        /// <param name="msg"></param>
        public virtual void SetMsg(MsgStatus status, string msg)
        {
            this.Result.Rest();
            this.Result.Status = (int)status;
            this.Result.Msg = msg;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            this.Result = null;
        }
    }
}
