using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutomatedQA.TestPlan;
using GoTesting.ToolConfig;

namespace GoTesting
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (args == null || args.Length == 0)
                {
                    //window modal
                    Application.Run(new Form1());
                }
                //console modal
                else if (args.Length == 1 && args[0].ToLower().Contains("autorun"))
                {
                    Application.Run(new TestingController());
                }
            }
            catch (Exception ex)
            {
                string strDateInfo = "Unhandled Exception: " + DateTime.Now + "\r\n";

                string str = string.Format(strDateInfo + "Exception Type: {0}\r\nMessage: {1}\r\nInfomation: {2}\r\n",
                    ex.GetType().Name, ex.Message, ex.StackTrace);
                MessageBox.Show(str, @"System Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            string str;
            string strDateInfo = "Unhandled Exception: " + DateTime.Now + "\r\n";
            Exception error = e.Exception;
            if (error != null)
            {
                str = string.Format(strDateInfo + "Exception Type: {0}\r\nMessage: {1}\r\nInfomation: {2}\r\n",
                     error.GetType().Name, error.Message, error.StackTrace);
            }
            else
            {
                str = string.Format("Thread Exception: {0}", e);
            }
            MessageBox.Show(str, @"System Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception error = e.ExceptionObject as Exception;
            string strDateInfo = "Unhandled Exception: " + DateTime.Now + "\r\n";
            string str = error != null ? string.Format(strDateInfo + "Application UnhandledException:{0};\n\rStackTrace: {1}", error.Message, error.StackTrace) : string.Format("Application UnhandledError:{0}", e);

            MessageBox.Show(str, @"System Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
