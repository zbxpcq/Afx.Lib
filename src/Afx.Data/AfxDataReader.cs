using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.Common;
using System.IO;
using System.Threading;
#if NET452 || NET462 || NET472 || NETSTANDARD || NETCOREAPP
using System.Threading.Tasks;
#endif

namespace Afx.Data
{
    /// <summary>
    /// Reads a forward-only stream of rows from a data source.
    /// </summary>
    public sealed class AfxDataReader : DbDataReader, IDataReader
    {
        private DbDataReader reader;
        private Database db;
        private DbCommand dbCommand;

        internal AfxDataReader(Database db, DbDataReader reader, DbCommand dbCommand)
        {
            this.db = db;
            this.reader = reader;
            this.dbCommand = dbCommand;
        }

        /// <summary>
        /// Closes the System.Data.Common.DbDataReader object.
        /// </summary>
        public override void Close()
        {
            if (this.dbCommand != null) this.dbCommand.Dispose();
            if (this.reader != null) this.reader.Close();
            if(this.db != null) this.db.Close();
            this.db = null;
        }

        /// <summary>
        /// Gets a value indicating the depth of nesting for the current row.
        /// </summary>
        public override int Depth
        {
            get { return this.reader.Depth; }
        }
        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        public override int FieldCount
        {
            get { return this.reader.FieldCount; }
        }
        /// <summary>
        ///  Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override bool GetBoolean(int ordinal)
        {
            return this.reader.GetBoolean(ordinal);
        }

        /// <summary>
        /// Gets the value of the specified column as a byte.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override byte GetByte(int ordinal)
        {
            return this.reader.GetByte(ordinal);
        }
        /// <summary>
        /// Reads a stream of bytes from the specified column, starting at location indicated by dataOffset, into the buffer, starting at the location indicated by bufferOffset.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <param name="dataOffset">The index within the row from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which to copy the data.</param>
        /// <param name="bufferOffset">The index with the buffer to which the data will be copied.</param>
        /// <param name="length">The maximum number of characters to read.</param>
        /// <returns>The actual number of bytes read.</returns>
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return this.reader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// Gets the value of the specified column as a single character.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns> The value of the specified column.</returns>
        public override char GetChar(int ordinal)
        {
            return this.reader.GetChar(ordinal);
        }

        /// <summary>
        /// Reads a stream of characters from the specified column, starting at location indicated by dataOffset, into the buffer, starting at the location indicated by bufferOffset.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <param name="dataOffset">The index within the row from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which to copy the data.</param>
        /// <param name="bufferOffset">The index with the buffer to which the data will be copied.</param>
        /// <param name="length">The maximum number of characters to read.</param>
        /// <returns>The actual number of characters read.</returns>
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return this.reader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// Gets name of the data type of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>A string representing the name of the data type.</returns>
        public override string GetDataTypeName(int ordinal)
        {
            return this.reader.GetDataTypeName(ordinal);
        }

        /// <summary>
        /// Gets the value of the specified column as a System.DateTime object.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override DateTime GetDateTime(int ordinal)
        {
            return this.reader.GetDateTime(ordinal);
        }

        /// <summary>
        /// Gets the value of the specified column as a System.Decimal object.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override decimal GetDecimal(int ordinal)
        {
            return this.reader.GetDecimal(ordinal);
        }
        /// <summary>
        /// Gets the value of the specified column as a double-precision floating point number.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns> The value of the specified column.</returns>
        public override double GetDouble(int ordinal)
        {
            return this.reader.GetDouble(ordinal);
        }
        /// <summary>
        /// Returns an System.Collections.IEnumerator that can be used to iterate through
        ///  the rows in the data reader.
        /// </summary>
        /// <returns>An System.Collections.IEnumerator that can be used to iterate through the rows in the data reader.</returns>
        public override System.Collections.IEnumerator GetEnumerator()
        {
            return this.reader.GetEnumerator();
        }
        /// <summary>
        /// Gets the data type of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The data type of the specified column.</returns>
        public override Type GetFieldType(int ordinal)
        {
            return this.reader.GetFieldType(ordinal);
        }
        /// <summary>
        /// Gets the value of the specified column as a float object.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override float GetFloat(int ordinal)
        {
            return this.reader.GetFloat(ordinal);
        }
        /// <summary>
        /// Gets the value of the specified column as a Guid object.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override Guid GetGuid(int ordinal)
        {
            return this.reader.GetGuid(ordinal);
        }
        /// <summary>
        /// Gets the value of the specified column as a short object.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override short GetInt16(int ordinal)
        {
            return this.reader.GetInt16(ordinal);
        }
        /// <summary>
        /// Gets the value of the specified column as a int object.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override int GetInt32(int ordinal)
        {
            return this.reader.GetInt32(ordinal);
        }
        /// <summary>
        /// Gets the value of the specified column as a long object.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override long GetInt64(int ordinal)
        {
            return this.reader.GetInt64(ordinal);
        }
        /// <summary>
        /// Gets the name of the column, given the zero-based column ordinal.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override string GetName(int ordinal)
        {
            return this.reader.GetName(ordinal);
        }
        /// <summary>
        /// Gets the column ordinal given the name of the column.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <returns>The zero-based column ordinal.</returns>
        public override int GetOrdinal(string name)
        {
            return this.reader.GetOrdinal(name);
        }
        /// <summary>
        ///  Returns a System.Data.DataTable that describes the column metadata of the System.Data.Common.DbDataReader.
        /// </summary>
        /// <returns>A System.Data.DataTable that describes the column metadata.</returns>
        public override DataTable GetSchemaTable()
        {
            return this.reader.GetSchemaTable();
        }
        /// <summary>
        /// Gets the value of the specified column as a string object.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override string GetString(int ordinal)
        {
            return this.reader.GetString(ordinal);
        }
        /// <summary>
        /// Gets the value of the specified column as a object.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override object GetValue(int ordinal)
        {
            return this.reader.GetValue(ordinal);
        }
        /// <summary>
        /// Populates an array of objects with the column values of the current row.
        /// </summary>
        /// <param name="values">An array of System.Object into which to copy the attribute columns.</param>
        /// <returns>The number of instances of System.Object in the array.</returns>
        public override int GetValues(object[] values)
        {
            return this.reader.GetValues(values);
        }
        /// <summary>
        /// Gets a value that indicates whether this System.Data.Common.DbDataReader contains
        ///  one or more rows.
        /// </summary>
        public override bool HasRows
        {
            get { return this.reader.HasRows; }
        }
        /// <summary>
        ///  Gets a value indicating whether the System.Data.Common.DbDataReader is closed.
        /// </summary>
        public override bool IsClosed
        {
            get { return this.reader.IsClosed; }
        }
        /// <summary>
        /// Gets a value that indicates whether the column contains nonexistent or missing values.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>true if the specified column is equivalent to System.DBNull; otherwise false.</returns>
        public override bool IsDBNull(int ordinal)
        {
            return this.reader.IsDBNull(ordinal);
        }
        /// <summary>
        /// Advances the reader to the next result when reading the results of a batch of  statements.
        /// </summary>
        /// <returns>true if there are more result sets; otherwise false.</returns>
        public override bool NextResult()
        {
            return this.reader.NextResult();
        }
        /// <summary>
        /// Advances the reader to the next record in a result set.
        /// </summary>
        /// <returns>true if there are more rows; otherwise false.</returns>
        public override bool Read()
        {
            return this.reader.Read();
        }
        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </summary>
        public override int RecordsAffected
        {
            get { return this.reader.RecordsAffected; }
        }
        /// <summary>
        ///  Gets the value of the specified column as an instance of System.Object.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <returns>The value of the specified column.</returns>
        public override object this[string name]
        {
            get { return this.reader[name]; }
        }
        /// <summary>
        ///  Gets the value of the specified column as an instance of System.Object.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override object this[int ordinal]
        {
            get { return this.reader[ordinal]; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override int VisibleFieldCount
        {
            get
            {
                return this.reader.VisibleFieldCount;
            }
        }

#if !NETCOREAPP && !NETSTANDARD
        /// <summary>
        /// 
        /// </summary>
        public override System.Runtime.Remoting.ObjRef CreateObjRef(Type requestedType)
        {
            return this.reader.CreateObjRef(requestedType);
        }
#endif
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.reader.GetHashCode();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override Type GetProviderSpecificFieldType(int ordinal)
        {
            return this.reader.GetProviderSpecificFieldType(ordinal);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override object GetProviderSpecificValue(int ordinal)
        {
            return this.reader.GetProviderSpecificValue(ordinal);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public override int GetProviderSpecificValues(object[] values)
        {
            return this.reader.GetProviderSpecificValues(values);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override object InitializeLifetimeService()
        {
            return this.reader.InitializeLifetimeService();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        protected override DbDataReader GetDbDataReader(int ordinal)
        {
            return this.reader.GetData(ordinal);
        }
#if NET452 || NET462 || NET472 || NETSTANDARD || NETCOREAPP
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override T GetFieldValue<T>(int ordinal)
        {
            return this.reader.GetFieldValue<T>(ordinal);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override Stream GetStream(int ordinal)
        {
            return this.reader.GetStream(ordinal);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override TextReader GetTextReader(int ordinal)
        {
            return this.reader.GetTextReader(ordinal);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
        {
            return this.reader.IsDBNullAsync(ordinal, cancellationToken);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ordinal"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        {
            return this.reader.GetFieldValueAsync<T>(ordinal, cancellationToken);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
        {
            return this.reader.NextResultAsync(cancellationToken);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            return this.reader.ReadAsync(cancellationToken);
        }
#endif
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if(disposing) this.Close();
            base.Dispose(disposing);
        }
    }
}
