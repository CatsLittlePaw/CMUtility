using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CMUtility
{
    public class SqlCommonHelper
    {
        static String ConnectionString = ConfigurationManager.ConnectionStrings["SiteSqlServer"].ToString();
        SqlHelp SqlHelper = new SqlHelp();

        public SqlCommonHelper() { 
        
        }
        public List<T> QueryToList<T>(string sql, List<SqlParameter> parammeter) where T : new()
        {
            DataTable dt = SqlHelper.ExecuteDataTable(ConnectionString, sql, 60, parammeter.ToArray());
            return dt.ToList<T>();
        }
        public DataTable Query(string sql, List<SqlParameter> parammeter)
        {
            DataTable dt = SqlHelper.ExecuteDataTable(ConnectionString, sql, 60, parammeter.ToArray());
            return dt;
        }
        public int InsertUpdateQuery(MySqlParam model) {
            int result = 0;
            SqlConnection conn = new SqlConnection(ConnectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();     
            SqlTransaction transaction;
            transaction = conn.BeginTransaction("Transaction");
            command.Connection = conn;
            command.Transaction = transaction;
            try
            {
                command.CommandText = model.sql;
                command.Parameters.Clear();
                foreach (var P in model.SqlParameter) {
                    command.Parameters.Add(P);
                }
                result = command.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MyLog.WriteLog(ex.ToString());

                var LN = new LineNotify();
                LN.PostErrorMessage(ex.Message, "嚴重");

                return result;
            }
            return result;
        }
        public int InsertUpdateQuery(List<MySqlParam> models)
        {
            int result = 0;
            SqlConnection conn = new SqlConnection(ConnectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            SqlTransaction transaction;
            transaction = conn.BeginTransaction("Transaction");
            command.Connection = conn;
            command.Transaction = transaction;
            try
            {
                foreach (var model in models) {
                    command.CommandText = model.sql;
                    command.Parameters.Clear();
                    foreach (var P in model.SqlParameter)
                    {
                        command.Parameters.Add(P);
                    }
                    result += command.ExecuteNonQuery();
                }
                
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                result = 0;
                MyLog.WriteLog(ex.ToString());

                var LN = new LineNotify();
                LN.PostErrorMessage(ex.Message, "嚴重");

                return result;
            }
            return result;
        }

        public int ExecuteSP(MySqlParam model)
        {
            int result = 0;
            SqlConnection conn = new SqlConnection(ConnectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            SqlTransaction transaction;
            transaction = conn.BeginTransaction("Transaction");
            command.Connection = conn;
            command.Transaction = transaction;
            try
            {
                command.CommandText = model.StoredProcedure;
                command.Parameters.Clear();
                foreach (var P in model.SqlParameter)
                {
                    command.Parameters.Add(P);
                }
                result = command.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MyLog.WriteLog(ex.ToString());

                var LN = new LineNotify();
                LN.PostErrorMessage(ex.Message, "嚴重");

                return result;
            }
            return result;
        }

    }

    public class MySqlParam
    {
        public object obj { get; set; }
        public string sql { get; set; }
        public List<SqlParameter> SqlParameter { get; set; }
        public string StoredProcedure { get; set; }

        public MySqlParam()
        {
            SqlParameter = new List<SqlParameter>();
        }

        public MySqlParam(object obj)
        {
            this.obj = obj;
            SqlParameter = SqlParameterExtensions.ToSqlParamsList(obj);
        }

        public MySqlParam(object obj, CRUDType type)
        {
            this.obj = obj;
            SqlParameter = SqlParameterExtensions.ToSqlParamsList(obj);
            ToSql(type);
        }

        public void ToSql(CRUDType type)
        {
            sql = SqlParameterExtensions.ToSql(obj, type);
        }
    }    
}
