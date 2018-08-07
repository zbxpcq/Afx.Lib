using System;
using System.Reflection;

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
        /// <param name="obj">obj</param>
        /// <returns></returns>
        public virtual string To(IDatabase db, string sql, object obj)
        {
            string _sql = sql;
            if(obj != null)
            {
                var t = obj.GetType();
                var parr = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (var p in parr)
                {
                    string paramname = "@" + p.Name;// "$" + p.Name+ "$";
                    //if(!_sql.Contains(paramname)) paramname = "@" + p.Name;
                    if (_sql.Contains(paramname))
                    {
                        var pname = db.EncodeParameterName(p.Name);
                        if (paramname != pname)
                        {
                            _sql = _sql.Replace(paramname, pname);
                        }
                        var v = p.GetValue(obj, null);
                        db.AddParameter(pname, v);
                    }
                }
            }

            return _sql;
        }
    }
}
