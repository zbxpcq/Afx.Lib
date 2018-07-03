﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Afx.Data.Entity.Schema
{
    /// <summary>
    /// 表结构接口
    /// </summary>
    public interface ITableSchema : IDisposable
    {
        /// <summary>
        /// 执行的 sql logs
        /// </summary>
        Action<string> Log { get; set; }

        /// <summary>
        /// 获取所有表名
        /// </summary>
        /// <returns></returns>
        List<string> GetTables();

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="columns">列信息</param>
        /// <returns>是否创建成功</returns>
        bool CreateTable(string table, List<ColumnInfoModel> columns);

        //bool DeleteTable(string table);

            /// <summary>
            /// 获取表列信息
            /// </summary>
            /// <param name="table">表名</param>
            /// <returns>列信息</returns>
        List<ColumnInfoModel> GetTableColumns(string table);

        /// <summary>
        /// 添加列
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="column">列信息</param>
        /// <returns>是否添加成功</returns>
        bool AddColumn(string table, ColumnInfoModel column);

        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="columns">索引列</param>
        void AddIndex(string table, List<ColumnInfoModel> columns);

        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="column">索引列</param>
        /// <returns>是否添加成功</returns>
        bool AddIndex(string table, ColumnInfoModel column);

        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="indexName">索引名称</param>
        /// <param name="isUnique">是否唯一索引</param>
        /// <param name="columns">索引列</param>
        /// <returns>是否添加成功</returns>
        bool AddIndex(string table, string indexName, bool isUnique, List<string> columns);

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="index">索引名称</param>
        /// <returns></returns>
        bool DeleteIndex(string table, string index);

        // bool DeleteColumn(string table, ColumnInfoModel column);

        //bool AlterColumn(string table, ColumnInfoModel column);

        // bool EqualColumn(ColumnInfoModel column1, ColumnInfoModel column2);

        /// <summary>
        /// 获取所有model type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<Type> GetModelType<T>() where T: EntityContext;
        
        /// <summary>
        /// 获取表名
        /// </summary>
        /// <param name="modelType">model type</param>
        /// <returns>表名</returns>
        string GetTableName(Type modelType);

        /// <summary>
        /// 获取model 属性列信息
        /// </summary>
        /// <param name="modelType">model type</param>
        /// <param name="table">表名</param>
        /// <returns>列信息</returns>
        List<ColumnInfoModel> GetModelColumns(Type modelType, string table);

        /// <summary>
        /// 获取列信息
        /// </summary>
        /// <param name="property">model 属性</param>
        /// <param name="table">表名</param>
        /// <returns>列信息</returns>
        ColumnInfoModel GetColumnInfo(PropertyInfo property, string table);

        /// <summary>
        /// 获取列数据库类型
        /// </summary>
        /// <param name="propertyType">model 属性类型</param>
        /// <param name="maxLength">类型最大长度</param>
        /// <param name="minLength">类型最小长度</param>
        /// <returns>列数据库类型</returns>
        string GetColumnType(Type propertyType, int maxLength, int minLength);
    }
}
