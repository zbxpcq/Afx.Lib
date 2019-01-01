using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Afx.Data.Extensions
{
    /// <summary>
    /// sql 参数class 转换sqlParameter接口
    /// </summary>
    public class ModelToParam : IModelToParam
    {
        protected static readonly List<char> spChar = new List<char>() { ' ','=','<','>', ',',';', '(',')','\r','\n','\t' };
        /// <summary>
        /// obj转换sqlParameter
        /// </summary>
        /// <param name="db">IDatabase</param>
        /// <param name="sql">sql</param>
        /// <param name="parameters">obj</param>
        /// <returns></returns>
        public virtual string To(IDatabase db, string sql, object parameters)
        {
            string commandText = sql;
            if(parameters != null)
            {
                var t = parameters.GetType();
                if (t.IsClass)
                {
                    var parr = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var p in parr)
                    {
                        bool isparam = false;
                        if (db.CommandType == null || db.CommandType == System.Data.CommandType.Text)
                        {
                            string paramname = "@" + p.Name;
                            var pname = db.EncodeParameterName(p.Name);
                            int index = 0;
                            int sqlIndex = 0;
                            stringBuilder.Clear();
                            while (0 <= index && index < commandText.Length && (index = commandText.IndexOf(paramname, index)) >= 0)
                            {
                                if (index > 0)
                                {
                                    var c = commandText[index - 1];
                                    if (!spChar.Contains(c))
                                    {
                                        index = index + paramname.Length;
                                        continue;
                                    }
                                }
                                if (index + paramname.Length < commandText.Length)
                                {
                                    var c = commandText[index + paramname.Length];
                                    if (!spChar.Contains(c))
                                    {
                                        index = index + paramname.Length;
                                        continue;
                                    }
                                }

                                isparam = true;
                                if (paramname == pname)
                                {
                                    break;
                                }
                                else
                                {
                                    stringBuilder.Append(commandText.Substring(sqlIndex, index));
                                    stringBuilder.Append(pname);
                                    sqlIndex = index = index + paramname.Length;
                                }
                            }

                            if (0 < sqlIndex && sqlIndex < commandText.Length)
                            {
                                stringBuilder.Append(commandText.Substring(sqlIndex));
                                commandText = stringBuilder.ToString();
                            }
                        }
                        else
                        {
                            isparam = true;
                        }

                        if (isparam)
                        {
                            System.Data.DbType? dbtype = null;
                            var v = p.GetValue(parameters, null);
                            if (v == null && p.PropertyType == typeof(byte[])) dbtype = System.Data.DbType.Binary;
                            db.AddParameter(p.Name, v, dbtype);
                        }
                    }
                }
            }

            return commandText;
        }
    }

    public class DicToParam : ModelToParam
    {
        /// <summary>
        /// obj转换sqlParameter
        /// </summary>
        /// <param name="db">IDatabase</param>
        /// <param name="sql">sql</param>
        /// <param name="parameters">obj</param>
        /// <returns></returns>
        public override string To(IDatabase db, string sql, object parameters)
        {
            string commandText = sql;
            if (parameters != null && parameters is IEnumerable<KeyValuePair<string, object>>)
            {
                var dic = parameters as IEnumerable<KeyValuePair<string, object>>;
                StringBuilder stringBuilder = new StringBuilder();
                foreach (KeyValuePair<string, object> kv in dic)
                {
                    var key = kv.Key.TrimStart('@');
                    bool isparam = false;
                    if (db.CommandType == null || db.CommandType == System.Data.CommandType.Text)
                    {
                        string paramname = "@" + key;
                        var pname = db.EncodeParameterName(key);
                        int index = 0;
                        int sqlIndex = 0;
                        stringBuilder.Clear();
                        while (0 <= index && index < commandText.Length && (index = commandText.IndexOf(paramname, index)) >= 0)
                        {
                            if (index > 0)
                            {
                                var c = commandText[index - 1];
                                if (!spChar.Contains(c))
                                {
                                    index = index + paramname.Length;
                                    continue;
                                }
                            }
                            if (index + paramname.Length < commandText.Length)
                            {
                                var c = commandText[index + paramname.Length];
                                if (!spChar.Contains(c))
                                {
                                    index = index + paramname.Length;
                                    continue;
                                }
                            }

                            isparam = true;
                            if (paramname == pname)
                            {
                                break;
                            }
                            else
                            {
                                stringBuilder.Append(commandText.Substring(sqlIndex, index));
                                stringBuilder.Append(pname);
                                sqlIndex = index = index + paramname.Length;
                            }
                        }

                        if (0 < sqlIndex && sqlIndex < commandText.Length)
                        {
                            stringBuilder.Append(commandText.Substring(sqlIndex));
                            commandText = stringBuilder.ToString();
                        }
                    }
                    else
                    {
                        isparam = true;
                    }

                    if (isparam)
                    {
                        db.AddParameter(key, kv.Value);
                    }
                }
            }

            return commandText;
        }
    }
}
