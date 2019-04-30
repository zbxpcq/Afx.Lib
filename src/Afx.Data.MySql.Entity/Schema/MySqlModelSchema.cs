﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Afx.Data.Entity.Schema;

namespace Afx.Data.MySql.Entity.Schema
{
    /// <summary>
    /// 获取model信息
    /// </summary>
    public sealed class MySqlModelSchema : ModelSchema
    {
        /// <summary>
        /// 获取model属性类型string
        /// </summary>
        /// <param name="column">列信息</param>
        /// <returns>model属性类型string</returns>
        public override string GetPropertyType(ColumnInfoModel column)
        {
            if (column == null) throw new ArgumentNullException("column");
            string type = "";
            DataTypeModel m = null;
            dic.TryGetValue(column.DataType, out m);
            if (m != null)
            {
                type = column.IsNullable ? m.NullableType : m.Type;
            }

            return type;
        }

        private static Dictionary<string, DataTypeModel> dic = new Dictionary<string, DataTypeModel>(StringComparer.OrdinalIgnoreCase);

        static MySqlModelSchema()
        {
            dic.Add("int", new DataTypeModel() { DbDataType = "int", Type = "int", NullableType = "Nullable<int>" });
            dic.Add("mediumint", new DataTypeModel() { DbDataType = "mediumint", Type = "int", NullableType = "Nullable<int>" });
            dic.Add("bigint", new DataTypeModel() { DbDataType = "bigint", Type = "long", NullableType = "Nullable<long>" });
            dic.Add("bool", new DataTypeModel() { DbDataType = "bool", Type = "bool", NullableType = "Nullable<bool>" });
            dic.Add("boolean", new DataTypeModel() { DbDataType = "boolean", Type = "bool", NullableType = "Nullable<bool>" });
            dic.Add("smallint", new DataTypeModel() { DbDataType = "smallint", Type = "short", NullableType = "Nullable<short>" });
            dic.Add("year", new DataTypeModel() { DbDataType = "year", Type = "short", NullableType = "Nullable<short>" });
            dic.Add("tinyint", new DataTypeModel() { DbDataType = "tinyint", Type = "byte", NullableType = "Nullable<byte>" });


            dic.Add("numeric", new DataTypeModel() { DbDataType = "numeric", Type = "decimal", NullableType = "Nullable<decimal>" });
            dic.Add("decimal", new DataTypeModel() { DbDataType = "decimal", Type = "decimal", NullableType = "Nullable<decimal>" });
            dic.Add("double", new DataTypeModel() { DbDataType = "double", Type = "double", NullableType = "Nullable<double>" });
            dic.Add("float", new DataTypeModel() { DbDataType = "float", Type = "float", NullableType = "Nullable<float>" });
            dic.Add("real", new DataTypeModel() { DbDataType = "real", Type = "double", NullableType = "Nullable<double>" });

            dic.Add("date", new DataTypeModel() { DbDataType = "date", Type = "DateTime", NullableType = "Nullable<DateTime>" });
            dic.Add("datetime", new DataTypeModel() { DbDataType = "datetime", Type = "DateTime", NullableType = "Nullable<DateTime>" });
            dic.Add("timestamp", new DataTypeModel() { DbDataType = "timestamp", Type = "TimeSpan", NullableType = "Nullable<TimeSpan>" });
            dic.Add("time", new DataTypeModel() { DbDataType = "time", Type = "TimeSpan", NullableType = "Nullable<TimeSpan>" });

            dic.Add("char", new DataTypeModel() { DbDataType = "char", Type = "string", NullableType = "string" });
            dic.Add("mediumtext", new DataTypeModel() { DbDataType = "mediumtext", Type = "string", NullableType = "string" });
            dic.Add("varchar", new DataTypeModel() { DbDataType = "varchar", Type = "string", NullableType = "string" });
            dic.Add("longtext", new DataTypeModel() { DbDataType = "longtext", Type = "string", NullableType = "string" });
            dic.Add("text", new DataTypeModel() { DbDataType = "text", Type = "string", NullableType = "string" });
            dic.Add("enum", new DataTypeModel() { DbDataType = "enum", Type = "string", NullableType = "string" });
            dic.Add("tinytext", new DataTypeModel() { DbDataType = "tinytext", Type = "string", NullableType = "string" });
            dic.Add("bit", new DataTypeModel() { DbDataType = "bit", Type = "bool", NullableType = "Nullable<bool>" });

            dic.Add("binary", new DataTypeModel() { DbDataType = "binary", Type = "byte[]", NullableType = "byte[]" });
            dic.Add("varbinary", new DataTypeModel() { DbDataType = "varbinary", Type = "byte[]", NullableType = "byte[]" });
            dic.Add("blob", new DataTypeModel() { DbDataType = "blob", Type = "byte[]", NullableType = "byte[]" });
            dic.Add("longblob", new DataTypeModel() { DbDataType = "longblob", Type = "byte[]", NullableType = "byte[]" });
            dic.Add("mediumblob", new DataTypeModel() { DbDataType = "mediumblob", Type = "byte[]", NullableType = "byte[]" });
            dic.Add("tinyblob", new DataTypeModel() { DbDataType = "tinyblob", Type = "byte[]", NullableType = "byte[]" });
        }
    }
}
