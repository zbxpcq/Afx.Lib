using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afx.Data.Entity.Schema
{
    /// <summary>
    /// 用于更新数据库结构类型
    /// </summary>
    public class BuildDatabase : IBuildDatabase
    {
        private IDatabaseSchema databaseSchema;
        private ITableSchema tableSchema;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="databaseSchema">获取数据库结构信息接口</param>
        /// <param name="tableSchema">表架构接口</param>
        public BuildDatabase(IDatabaseSchema databaseSchema, ITableSchema tableSchema)
        {
            if (databaseSchema == null) throw new ArgumentNullException("databaseSchema");
            if(tableSchema == null) throw new ArgumentNullException("tableSchema");
            this.databaseSchema = databaseSchema;
            this.tableSchema = tableSchema;
        }

        /// <summary>
        /// 创建、更新数据库结构，添加 更新、删除索引， 列只增加，不修改，不删除
        /// </summary>
        /// <typeparam name="T">EntityContext</typeparam>
        public virtual void Build<T>() where T : EntityContext
        {
            Type dbContextType = typeof(T);
            bool count = databaseSchema.Exist();
            if (count == false)
                databaseSchema.CreateDatabase();

            List<string> tables = tableSchema.GetTables();
            List<Type> modelTypes = tableSchema.GetModelType(dbContextType);
            foreach (var t in modelTypes)
            {
                string tb = tableSchema.GetTableName(t);
                if (!tables.Contains(tb, StringComparer.OrdinalIgnoreCase))
                {
                    var columns = tableSchema.GetModelColumns(t, tb);
                    tableSchema.CreateTable(tb, columns);
                }
                else
                {
                    var tableColumns = tableSchema.GetTableColumns(tb);
                    var modelColumns = tableSchema.GetModelColumns(t, tb);
                    List<ColumnInfoModel> addIndexs = new List<ColumnInfoModel>();
                    List<string> delIndexs = new List<string>();
                    foreach (var mColumn in modelColumns)
                    {
                        var tColumn = tableColumns.Find(q => string.Compare(q.Name, mColumn.Name, true) == 0);
                        if (tColumn == null)
                        {
                            tableSchema.AddColumn(tb, mColumn);
                        }

                        if (!mColumn.IsKey)
                        {
                            if(tColumn != null && tColumn.IndexName == null) tColumn.IndexName = string.Empty;
                            if(mColumn.IndexName == null) mColumn.IndexName = string.Empty;

                            if (!string.IsNullOrEmpty(mColumn.IndexName)
                                && (tColumn == null || tColumn != null
                                && (!string.Equals(mColumn.IndexName, tColumn.IndexName,
                                StringComparison.OrdinalIgnoreCase)
                                || mColumn.IsUnique != tColumn.IsUnique)))
                            {
                                addIndexs.Add(mColumn);
                            }

                            if (tColumn != null && !string.IsNullOrEmpty(tColumn.IndexName)
                                && (!string.Equals(tColumn.IndexName, mColumn.IndexName, StringComparison.OrdinalIgnoreCase)
                                || mColumn.IsUnique != tColumn.IsUnique))
                            {
                                if (!delIndexs.Contains(tColumn.IndexName, StringComparer.OrdinalIgnoreCase))
                                    delIndexs.Add(tColumn.IndexName);
                            }
                        }

                    }
                    foreach(var indexs in delIndexs)
                    {
                        tableSchema.DeleteIndex(tb, indexs);
                    }
                    tableSchema.AddIndex(tb, addIndexs);
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (databaseSchema != null) databaseSchema.Dispose();
            if (tableSchema != null) tableSchema.Dispose();

            this.databaseSchema = null;
            this.tableSchema = null;
        }
    }
}
