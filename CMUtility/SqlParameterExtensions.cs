using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace CMUtility
{
    /// <summary>
    /// Use for an alternative param name other than the propery name
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property)]
    public class QueryParamNameAttribute : Attribute
    {
        public string Name { get; set; }
        public QueryParamNameAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Ignore this property
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property)]
    public class QueryParamIgnoreAttribute : Attribute
    {
    }

    /// <summary>
    /// Primary Key property
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {       
        public PrimaryKeyAttribute()
        {
            
        }
    }

    public class QueryParamInfo
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

    public class SqlInfo
    {
        public SqlInfo()
        {
            Columns = new List<string>();
            Parameters = new List<string>();
            PrimaryKeys = new List<string>();
        }

        public string TableName { get; set; }

        public List<string> Columns { get; set; }

        public List<string> Parameters { get; set; }

        public List<string> PrimaryKeys {  get; set; }
    }

    public static class SqlParameterExtensions
    {
        public static object[] ToSqlParamsArray(this object obj, SqlParameter[] additionalParams = null)
        {
            var result = ToSqlParamsList(obj, additionalParams);
            return result.ToArray<object>();

        }

        public static List<SqlParameter> ToSqlParamsList(this object obj, SqlParameter[] additionalParams = null)
        {
            var props = (
                from p in obj.GetType().GetProperties()
                let nameAttr = p.GetCustomAttributes(typeof(QueryParamNameAttribute), true)
                let ignoreAttr = p.GetCustomAttributes(typeof(QueryParamIgnoreAttribute), true)
                select new { Property = p, Names = nameAttr, Ignores = ignoreAttr }).ToList();

            var result = new List<SqlParameter>();

            props.ForEach(p =>
            {
                if (p.Ignores != null && p.Ignores.Length > 0)
                    return;
                var name = p.Names.FirstOrDefault() as QueryParamNameAttribute;
                var pinfo = new QueryParamInfo();
                if (name != null && !String.IsNullOrWhiteSpace(name.Name))
                    pinfo.Name = name.Name.Replace("@", "");
                else
                    pinfo.Name = p.Property.Name.Replace("@", "");
                pinfo.Value = p.Property.GetValue(obj) ?? DBNull.Value;
                var sqlParam = new SqlParameter(pinfo.Name, TypeConvertor.ToSqlDbType(p.Property.PropertyType))
                {
                    Value = pinfo.Value
                };
                result.Add(sqlParam);
            });

            if (additionalParams != null && additionalParams.Length > 0)
                result.AddRange(additionalParams);

            return result;
        }

        public static string ToSql(this object obj, CRUDType type)
        {
            var props = (
               from p in obj.GetType().GetProperties()
               let nameAttr = p.GetCustomAttributes(typeof(QueryParamNameAttribute), true)
               let ignoreAttr = p.GetCustomAttributes(typeof(QueryParamIgnoreAttribute), true)
               let primaryKeyAttr = p.GetCustomAttributes(typeof(PrimaryKeyAttribute), true)
               select new { Property = p, Names = nameAttr, Ignores = ignoreAttr, PrimaryKeys = primaryKeyAttr }).ToList();

            var sinfo = new SqlInfo();

            sinfo.TableName = obj.GetType().Name;

            props.ForEach(p =>
            {     
                if (p.Ignores != null && p.Ignores.Length > 0)
                    return;
                var name = p.Names.FirstOrDefault() as QueryParamNameAttribute;
                var pk = p.PrimaryKeys.FirstOrDefault() as PrimaryKeyAttribute;

                if (name != null && !String.IsNullOrWhiteSpace(name.Name))
                {
                    sinfo.Parameters.Add($"@{name.Name}");
                    sinfo.Columns.Add(name.Name.Replace("@", ""));
                    if(pk != null)
                        sinfo.PrimaryKeys.Add(name.Name.Replace("@", ""));
                }                    
                else
                {
                    sinfo.Parameters.Add($"@{p.Property.Name}");
                    sinfo.Columns.Add(p.Property.Name.Replace("@", ""));
                    if (pk != null)
                        sinfo.PrimaryKeys.Add(p.Property.Name.Replace("@", ""));
                }               
            });

            return To(sinfo, type);
        }

        public static string To(SqlInfo sinfo, CRUDType type)
        {
            string result = string.Empty;
            string whereCond = "1=1 ";
            switch (type)
            {
                case CRUDType.C:
                    result += $"INSERT INTO {sinfo.TableName}({string.Join(", ", sinfo.Columns)}) VALUES({string.Join(", ", sinfo.Parameters)}); ";
                    break;
                case CRUDType.R:
                    if(sinfo.PrimaryKeys.Count > 0)
                        whereCond += $"AND {string.Join("AND ", (from item in sinfo.PrimaryKeys select $"{item} = @{item} ").ToList())}";                 
                    result += $"SELECT {string.Join(", ", sinfo.Columns)} FROM {sinfo.TableName} WHERE {whereCond}; ";
                    break;
                case CRUDType.U:
                    sinfo.Columns.AddRange(sinfo.PrimaryKeys);
                    result += $"UPDATE {sinfo.TableName} SET {string.Join(", ", (from item in sinfo.Columns.Distinct() select $"{item} = @{item} ").ToList())} WHERE ";
                    if (sinfo.PrimaryKeys.Count > 0)
                        whereCond += $"AND {string.Join("AND ", (from item in sinfo.PrimaryKeys select $"{item} = @{item} ").ToList())}";
                    result += $"{whereCond}; ";
                    break;
                case CRUDType.D:
                    // 避免誤將全部刪除
                    if(sinfo.PrimaryKeys.Count > 0)
                    {
                        whereCond += $"AND {string.Join("AND ", (from item in sinfo.PrimaryKeys select $"{item} = @{item} ").ToList())}";
                        result += $"DELETE FROM {sinfo.TableName} WHERE {whereCond}; ";
                    }                   
                    break;
                default:
                    break;
            }

            return result;
        }
    }


    //Convert .Net Type to SqlDbType or DbType and vise versa
    //This class can be useful when you make conversion between types .The class supports conversion between .Net Type , SqlDbType and DbType .
    //https://gist.github.com/abrahamjp/858392

    /// <summary>
    /// Convert a base data type to another base data type
    /// </summary>
    public sealed class TypeConvertor
    {

        private struct DbTypeMapEntry
        {
            public Type Type;
            public DbType DbType;
            public SqlDbType SqlDbType;
            public DbTypeMapEntry(Type type, DbType dbType, SqlDbType sqlDbType)
            {
                this.Type = type;
                this.DbType = dbType;
                this.SqlDbType = sqlDbType;
            }

        };

        private static ArrayList _DbTypeList = new ArrayList();

        #region Constructors

        static TypeConvertor()
        {
            DbTypeMapEntry dbTypeMapEntry
            = new DbTypeMapEntry(typeof(bool), DbType.Boolean, SqlDbType.Bit);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(byte), DbType.Double, SqlDbType.TinyInt);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(byte[]), DbType.Binary, SqlDbType.Image);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(DateTime), DbType.DateTime, SqlDbType.DateTime);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(Decimal), DbType.Decimal, SqlDbType.Decimal);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(double), DbType.Double, SqlDbType.Float);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(Guid), DbType.Guid, SqlDbType.UniqueIdentifier);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(Int16), DbType.Int16, SqlDbType.SmallInt);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(Int32), DbType.Int32, SqlDbType.Int);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(Int64), DbType.Int64, SqlDbType.BigInt);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(object), DbType.Object, SqlDbType.Variant);
            _DbTypeList.Add(dbTypeMapEntry);

            dbTypeMapEntry
            = new DbTypeMapEntry(typeof(string), DbType.String, SqlDbType.VarChar);
            _DbTypeList.Add(dbTypeMapEntry);

        }

        private TypeConvertor()
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Convert db type to .Net data type
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static Type ToNetType(DbType dbType)
        {
            DbTypeMapEntry entry = Find(dbType);
            return entry.Type;
        }

        /// <summary>
        /// Convert TSQL type to .Net data type
        /// </summary>
        /// <param name="sqlDbType"></param>
        /// <returns></returns>
        public static Type ToNetType(SqlDbType sqlDbType)
        {
            DbTypeMapEntry entry = Find(sqlDbType);
            return entry.Type;
        }

        /// <summary>
        /// Convert .Net type to Db type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DbType ToDbType(Type type)
        {
            DbTypeMapEntry entry = Find(type);
            return entry.DbType;
        }

        /// <summary>
        /// Convert TSQL data type to DbType
        /// </summary>
        /// <param name="sqlDbType"></param>
        /// <returns></returns>
        public static DbType ToDbType(SqlDbType sqlDbType)
        {
            DbTypeMapEntry entry = Find(sqlDbType);
            return entry.DbType;
        }

        /// <summary>
        /// Convert .Net type to TSQL data type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SqlDbType ToSqlDbType(Type type)
        {
            DbTypeMapEntry entry = Find(type);
            return entry.SqlDbType;
        }

        /// <summary>
        /// Convert DbType type to TSQL data type
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static SqlDbType ToSqlDbType(DbType dbType)
        {
            DbTypeMapEntry entry = Find(dbType);
            return entry.SqlDbType;
        }

        private static DbTypeMapEntry Find(Type type)
        {
            object retObj = null;
            for (int i = 0; i < _DbTypeList.Count; i++)
            {
                DbTypeMapEntry entry = (DbTypeMapEntry)_DbTypeList[i];
                if (entry.Type == (Nullable.GetUnderlyingType(type) ?? type))
                {
                    retObj = entry;
                    break;
                }
            }
            if (retObj == null)
            {
                throw
                new ApplicationException("Referenced an unsupported Type " + type.ToString());
            }

            return (DbTypeMapEntry)retObj;
        }

        private static DbTypeMapEntry Find(DbType dbType)
        {
            object retObj = null;
            for (int i = 0; i < _DbTypeList.Count; i++)
            {
                DbTypeMapEntry entry = (DbTypeMapEntry)_DbTypeList[i];
                if (entry.DbType == dbType)
                {
                    retObj = entry;
                    break;
                }
            }
            if (retObj == null)
            {
                throw
                new ApplicationException("Referenced an unsupported DbType " + dbType.ToString());
            }

            return (DbTypeMapEntry)retObj;
        }

        private static DbTypeMapEntry Find(SqlDbType sqlDbType)
        {
            object retObj = null;
            for (int i = 0; i < _DbTypeList.Count; i++)
            {
                DbTypeMapEntry entry = (DbTypeMapEntry)_DbTypeList[i];
                if (entry.SqlDbType == sqlDbType)
                {
                    retObj = entry;
                    break;
                }
            }
            if (retObj == null)
            {
                throw
                new ApplicationException("Referenced an unsupported SqlDbType");
            }

            return (DbTypeMapEntry)retObj;
        }     

        #endregion
    }
}
