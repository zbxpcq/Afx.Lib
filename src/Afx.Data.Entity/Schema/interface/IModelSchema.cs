using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afx.Data.Entity.Schema
{
    /// <summary>
    /// 获取model信息
    /// </summary>
    public interface IModelSchema : IDisposable
    {
        /// <summary>
        /// 获取 model .cs 文件源代码
        /// </summary>
        /// <param name="modelName">model名称</param>
        /// <param name="columns">表列信息</param>
        /// <param name="namespace">model 命名空间</param>
        /// <returns>model .cs 文件源代码</returns>
        string GetModelCode(string modelName, List<ColumnInfoModel> columns, string @namespace);

        /// <summary>
        /// 获取model属性标记源代码
        /// </summary>
        /// <param name="column">列信息</param>
        /// <returns>model属性标记源代码</returns>
        string GetAttributeCode(ColumnInfoModel column);

        /// <summary>
        /// 获取model属性源代码
        /// </summary>
        /// <param name="column">列信息</param>
        /// <returns>model属性源代码</returns>
        string GetPropertyCode(ColumnInfoModel column);

        /// <summary>
        /// 获取model属性类型string
        /// </summary>
        /// <param name="column">列信息</param>
        /// <returns>model属性类型string</returns>
        string GetPropertyType(ColumnInfoModel column);

        /// <summary>
        /// 获取model名称
        /// </summary>
        /// <param name="table">表名</param>
        /// <returns>model名称</returns>
        string GetModelName(string table);

        /// <summary>
        /// 获取model属性名称
        /// </summary>
        /// <param name="column">列名</param>
        /// <returns>model属性名称</returns>
        string GetPropertyName(string column);
    }
}
