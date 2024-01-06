using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CMUtility
{
    public class SerialNumber
    {
        SqlCommonHelper SqlHelper = new SqlCommonHelper();

        public static string GetRandomString(int length)
        {
            var str = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var next = new Random();
            Thread.Sleep(200);
            var builder = "";
            for (var i = 0; i < length; i++)
            {
                builder += str[next.Next(0, str.Length)];
            }
            return builder;
        }
        public string GetNewGuidID(string TableName,string Head="ID", string Date = "",string NumLength = "000")
        {
            string dateText = (Date == "") ? DateTime.UtcNow.AddHours(8).ToString("yyyyMMdd") : Convert.ToDateTime(Date).ToString("yyyyMMdd");
            string IDHead = Head + dateText; // ID20200417 or MM20200417

            DataTable DT = GetAllMaxID(TableName, IDHead);
            if (DT.Rows.Count == 0)
                return IDHead + 1.ToString(NumLength);
            else if (DT.Rows[0]["ID"].ToString() == "NULL" || DT.Rows[0]["ID"].ToString() == "")
                return IDHead + 1.ToString(NumLength);
            else
            {
                String rID = DT.Rows[0]["ID"].ToString(); //DT.Rows[0]["ID"].ToString()

                return IDHead + (Convert.ToInt32(rID.Replace(IDHead, "")) + 1).ToString(NumLength);
            }
        }
        public DataTable GetAllMaxID(String TableName, String NowDate)
        {
            string sql = @"SELECT Max(ID) as ID
                            FROM [" + TableName + @"]
                            WHERE ID like @ID";
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@ID", NowDate + "%"));

            var dt = SqlHelper.Query(sql, parameters);
            return dt;
        }
    }
}
