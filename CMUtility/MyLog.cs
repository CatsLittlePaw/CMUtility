using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMUtility
{
    public class MyLog
    {
        public static void WriteLog(string message,string message2 = "")
        {
            string DIRNAME = AppDomain.CurrentDomain.BaseDirectory + @"\Log\";
            string FILENAME = DIRNAME + DateTime.UtcNow.AddHours(8).ToString("yyyyMMdd") + ".txt";

            if (!Directory.Exists(DIRNAME))
                Directory.CreateDirectory(DIRNAME); 

            if (!File.Exists(FILENAME))
            {
                // The File.Create method creates the file and opens a FileStream on the file. You neeed to close it.
                File.Create(FILENAME).Close();
            }
            using (StreamWriter sw = File.AppendText(FILENAME))
            {
                if (message2 == "") Log(message, sw);
                else Log(message, message2, sw);
            }
        }
        private static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.UtcNow.AddHours(8).ToLongTimeString(), DateTime.UtcNow.AddHours(8).ToLongDateString());
            w.WriteLine("  :");
            w.WriteLine("  :{0}", logMessage);
            w.WriteLine("-------------------------------");
        }
        private static void Log(string logMessage,string logMessage2, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.UtcNow.AddHours(8).ToLongTimeString(), DateTime.UtcNow.AddHours(8).ToLongDateString());
            w.WriteLine("  :");
            w.WriteLine("  :{0}", logMessage);
            w.WriteLine("傳入JSON:{0}", logMessage2);
            w.WriteLine("-------------------------------");
        }
    }
}
