using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;

namespace Afx.Data
{
    /// <summary>
    /// OleDb
    /// </summary>
    public sealed class OleDbDatabase : Database
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        public OleDbDatabase(string connectionString)
            : base(connectionString, OleDbFactory.Instance)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="isOwnsConnection"></param>
        public OleDbDatabase(OleDbConnection connection, bool isOwnsConnection = true)
            : base(connection, isOwnsConnection)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string EncodeParameterName(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            return string.Format("@{0}", name);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public override string EncodeColumn(string column)
        {
            if (string.IsNullOrEmpty(column)) throw new ArgumentNullException("column");
            return string.Format("[{0}]", column);
        }

    }
}
