using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Afx.Data.Extensions
{
    /// <summary>
    /// sql 参数class 转换sqlParameter接口
    /// </summary>
    public class ModelToParam : IModelToParam
    {
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
            if (parameters != null)
            {
                var t = parameters.GetType();
                if (t.IsClass)
                {
                    var parr = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var p in parr)
                    {
                        bool addParam = false;
                        var pname = db.EncodeParameterName(p.Name);
                        if (db.CommandType == null || db.CommandType == CommandType.Text)
                        {
                            var name = "@" + p.Name;
                            var regex = new Regex($"(\\s|[=<>;,\\(\\)]|^){name}(\\s|[=<>;,\\(\\)]|$)");
                            if (regex.IsMatch(commandText))
                            {
                                addParam = true;
                                if (pname != name)
                                {
                                    do
                                    {
                                        commandText = regex.Replace(commandText, (match) =>
                                        {
                                            return match.Value.Replace(name, pname);
                                        });
                                    } while (regex.IsMatch(commandText));
                                }
                            }
                        }
                        else if(db.CommandType == CommandType.StoredProcedure)
                        {
                            addParam = true;
                        }

                        if (addParam)
                        {
                            var v = p.GetValue(parameters, null);
                            var parameter = db.CreateParameter(pname, v ?? DBNull.Value);
                            DbType dbType;
                            if (DatabaseExtension.dbTypeDic.TryGetValue(p.PropertyType, out dbType))
                                parameter.DbType = dbType;
                            db.AddParameter(parameter);
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
                    bool addParam = false;
                    var key = kv.Key.TrimStart('@');
                    var pname = db.EncodeParameterName(key);
                    if (db.CommandType == null || db.CommandType == CommandType.Text)
                    {
                        string name = "@" + key;
                        var regex = new Regex($"(\\s|[=<>;,\\(\\)]|^){name}(\\s|[=<>;,\\(\\)]|$)");
                        if (regex.IsMatch(commandText))
                        {
                            if (pname != name)
                            {
                                do
                                {
                                    commandText = regex.Replace(commandText, (match) =>
                                    {
                                        return match.Value.Replace(name, pname);
                                    });
                                } while (regex.IsMatch(commandText));
                            }

                        }
                    }
                    else if (db.CommandType == CommandType.StoredProcedure)
                    {
                        addParam = true;
                    }

                    if (addParam)
                    {
                        var parameter = db.CreateParameter(pname, kv.Value ?? DBNull.Value);
                        DbType dbType;
                        if (kv.Value != null && DatabaseExtension.dbTypeDic.TryGetValue(kv.Value.GetType(), out dbType))
                            parameter.DbType = dbType;
                        db.AddParameter(parameter);
                    }
                }
            }

            return commandText;
        }
    }
}
