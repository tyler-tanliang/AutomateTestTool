using System;
using System.IO;

namespace GoTesting
{
    class Log
    {
        public static string LogFileName { get; set; }

        public static void Write(string info)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(LogFileName, true))
                {
                    sw.WriteLine(DateTime.Now.ToString("HH:mm:ss ") + info);
                }
            }
            catch (Exception ex)
            {
                //throw (new IOException("Write Log error file " + LogFileName + " is failed, information is:" + ex.Message));
            }
        }

        public static void Write(string format, params object[] param)
        {
            Write(string.Format(format, param));
        }
    }
}
