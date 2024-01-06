using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMUtility
{
    public class SqlHelp
    {
        public DataTable ExecuteDataTable(string Connection, string SqlStr, int CommandTimeOut, SqlParameter[] commandParameters = null)
        {

            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            SqlConnection icn = new SqlConnection();

            icn.ConnectionString = Connection;
            icn.Open();
            cmd.Connection = icn;
            cmd.CommandText = SqlStr;
            cmd.CommandTimeout = CommandTimeOut;
            if (commandParameters != null)
            {
                for (int i = 0; i < commandParameters.Length; i++)
                {
                    cmd.Parameters.Add(commandParameters[i]);
                }
            }
            SqlDataReader sr = cmd.ExecuteReader();
            dt.Load(sr);
            sr.Close();
            if (icn.State == ConnectionState.Open) icn.Close();
            cmd.Parameters.Clear();
            return dt;
        }
        public int ExecuteScalar(string Connection, string SqlStr, int CommandTimeOut, SqlParameter[] commandParameters = null)
        {
            object sr;
            int Count = 0;
            SqlCommand cmd = new SqlCommand();
            SqlConnection icn = new SqlConnection();

            icn.ConnectionString = Connection;
            icn.Open();
            cmd.Connection = icn;
            cmd.CommandText = SqlStr;
            cmd.CommandTimeout = CommandTimeOut;
            if (commandParameters != null)
            {
                for (int i = 0; i < commandParameters.Length; i++)
                {
                    cmd.Parameters.Add(commandParameters[i]);
                }
            }
            sr = cmd.ExecuteScalar();
            int.TryParse(sr.ToString(), out Count);

            cmd.Dispose();
            if (icn.State == ConnectionState.Open) icn.Close();


            return Count;
        }
        public void ExecuteNonQuery(string Connection, string SqlStr, int CommandTimeOut, SqlParameter[] commandParameters = null)
        {
            object sr;
            int Count = 0;
            SqlCommand cmd = new SqlCommand();
            SqlConnection icn = new SqlConnection();

            icn.ConnectionString = Connection;
            icn.Open();
            cmd.Connection = icn;
            cmd.CommandText = SqlStr;
            cmd.CommandTimeout = CommandTimeOut;
            if (commandParameters != null)
            {
                for (int i = 0; i < commandParameters.Length; i++)
                {
                    cmd.Parameters.Add(commandParameters[i]);
                }
            }
            sr = cmd.ExecuteNonQuery();
            int.TryParse(sr.ToString(), out Count);

            cmd.Dispose();
            if (icn.State == ConnectionState.Open) icn.Close();


        }
        public DataSet ExecuteDataset(string Connection, string SqlStr, int CommandTimeOut, SqlParameter[] commandParameters = null)
        {
            SqlConnection icn = new SqlConnection();

            icn.ConnectionString = Connection;
            icn.Open();
            SqlCommand conn = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter(conn);
            conn.Connection = icn;
            conn.CommandText = SqlStr;
            if (commandParameters != null)
            {
                foreach (var item in commandParameters)
                {
                    conn.Parameters.Add(item);
                }
            }
            conn.CommandTimeout = CommandTimeOut;
            DataSet ds = new DataSet();
            ds.Clear();
            da.Fill(ds);
            if (icn.State == ConnectionState.Open) icn.Close();
            return ds;
        }
        public object ExecuteScalarobject(string Connection, string SqlStr, int CommandTimeOut, SqlParameter[] commandParameters = null)
        {
            object sr;

            SqlCommand cmd = new SqlCommand();
            SqlConnection icn = new SqlConnection();

            icn.ConnectionString = Connection;
            icn.Open();
            cmd.Connection = icn;
            cmd.CommandText = SqlStr;
            cmd.CommandTimeout = CommandTimeOut;
            if (commandParameters != null)
            {
                for (int i = 0; i < commandParameters.Length; i++)
                {
                    cmd.Parameters.Add(commandParameters[i]);
                }
            }
            sr = cmd.ExecuteScalar();


            cmd.Dispose();
            if (icn.State == ConnectionState.Open) icn.Close();


            return sr;
        }

        public string ExecuteTransaction(string Connection, string SqlStr, int CommandTimeOut, SqlParameter[] commandParameters = null)
        {
            string NewId = "";
            string Sql = SqlStr;
            using (SqlConnection conn = new SqlConnection(Connection))
            {

                conn.Open();
                SqlCommand command = conn.CreateCommand();
                SqlTransaction transaction;
                transaction = conn.BeginTransaction("Transaction");
                command.Connection = conn;
                command.Transaction = transaction;
                try
                {


                    command.CommandText = Sql;
                    if (commandParameters != null)
                    {
                        for (int i = 0; i < commandParameters.Length; i++)
                        {
                            command.Parameters.Add(commandParameters[i]);
                        }
                    }
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();


                    transaction.Commit();

                    command.Dispose();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    NewId = ex.Message;
                    throw (ex);
                }
            }
            return NewId;
        }
    }
}
