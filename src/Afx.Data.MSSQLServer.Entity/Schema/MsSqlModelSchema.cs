using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Afx.Data.Entity.Schema;

namespace Afx.Data.MSSQLServer.Entity.Schema
{
    /// <summary>
    /// 获取model信息
    /// </summary>
    public sealed class MsSqlModelSchema : ModelSchema
    {
        private static Dictionary<string, DataTypeModel> dic = new Dictionary<string, DataTypeModel>(StringComparer.OrdinalIgnoreCase);
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

        static MsSqlModelSchema()
        {
            dic.Add("int", new DataTypeModel() { DbDataType = "int", Type = "int", NullableType = "Nullable<int>" });
            dic.Add("bigint", new DataTypeModel() { DbDataType = "bigint", Type = "long", NullableType = "Nullable<long>" });
            dic.Add("bit", new DataTypeModel() { DbDataType = "bit", Type = "bool", NullableType = "Nullable<bool>" });
            dic.Add("smallint", new DataTypeModel() { DbDataType = "smallint", Type = "short", NullableType = "Nullable<short>" });
            dic.Add("tinyint", new DataTypeModel() { DbDataType = "tinyint", Type = "byte", NullableType = "Nullable<byte>" });
            
            dic.Add("numeric", new DataTypeModel() { DbDataType = "numeric", Type = "decimal", NullableType = "Nullable<decimal>" });
            dic.Add("decimal", new DataTypeModel() { DbDataType = "decimal", Type = "decimal", NullableType = "Nullable<decimal>" });
            dic.Add("money", new DataTypeModel() { DbDataType = "money", Type = "decimal", NullableType = "Nullable<decimal>" });
            dic.Add("smallmoney", new DataTypeModel() { DbDataType = "smallmoney", Type = "float", NullableType = "Nullable<float>" });
            dic.Add("float", new DataTypeModel() { DbDataType = "float", Type = "double", NullableType = "Nullable<double>" });
            dic.Add("real", new DataTypeModel() { DbDataType = "real", Type = "float", NullableType = "Nullable<float>" });
            
            dic.Add("date", new DataTypeModel() { DbDataType = "date", Type = "DateTime", NullableType = "Nullable<DateTime>" });
            dic.Add("datetime", new DataTypeModel() { DbDataType = "datetime", Type = "DateTime", NullableType = "Nullable<DateTime>" });
            dic.Add("datetime2", new DataTypeModel() { DbDataType = "datetime2", Type = "DateTime", NullableType = "Nullable<DateTime>" });
            dic.Add("datetimeoffset", new DataTypeModel() { DbDataType = "datetimeoffset", Type = "DateTimeOffset", NullableType = "Nullable<DateTimeOffset>" });
            dic.Add("smalldatetime", new DataTypeModel() { DbDataType = "smalldatetime", Type = "DateTime", NullableType = "Nullable<DateTime>" });
            dic.Add("timestamp", new DataTypeModel() { DbDataType = "timestamp", Type = "TimeSpan", NullableType = "Nullable<TimeSpan>" });
            dic.Add("time", new DataTypeModel() { DbDataType = "time", Type = "TimeSpan", NullableType = "Nullable<TimeSpan>" });

            
            dic.Add("Uniqueidentifier", new DataTypeModel() { DbDataType = "Uniqueidentifier", Type = "Guid", NullableType = "Nullable<Guid>" });
            
            dic.Add("char", new DataTypeModel() { DbDataType = "char", Type = "string", NullableType = "string" });
            dic.Add("nchar", new DataTypeModel() { DbDataType = "nchar", Type = "string", NullableType = "string" });
            dic.Add("varchar", new DataTypeModel() { DbDataType = "varchar", Type = "string", NullableType = "string" });
            dic.Add("nvarchar", new DataTypeModel() { DbDataType = "nvarchar", Type = "string", NullableType = "string" });
            dic.Add("ntext", new DataTypeModel() { DbDataType = "ntext", Type = "string", NullableType = "string" });
            dic.Add("text", new DataTypeModel() { DbDataType = "text", Type = "string", NullableType = "string" });

            dic.Add("binary", new DataTypeModel() { DbDataType = "binary", Type = "byte[]", NullableType = "byte[]" });
            dic.Add("varbinary", new DataTypeModel() { DbDataType = "varbinary", Type = "byte[]", NullableType = "byte[]" });
            dic.Add("image", new DataTypeModel() { DbDataType = "image", Type = "byte[]", NullableType = "byte[]" });
        }
    }
}
