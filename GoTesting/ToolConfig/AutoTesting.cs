using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Xml.Linq;
//using AutomatedQA.Android;
//using AutomatedQA.DesktopApp;
using AutomatedQA.TestPlan;

namespace GoTesting.ToolConfig
{
    class AutoTesting
    {
        private static readonly object SynchronizingObject = new object();
        private static readonly string APKFileName = System.Configuration.ConfigurationManager.AppSettings["APKFileName"];
        private static readonly string SourceAPKFolder = System.Configuration.ConfigurationManager.AppSettings["SourceAPKFolder"];
        private static readonly string LocalAPKFolder = System.Configuration.ConfigurationManager.AppSettings["LocalAPKFolder"];
        //private static readonly string LocalInstallerPath = ConfigurationManager.AppSettings["LocalInstallerPath"];
        //private static readonly string ProduceCode = ConfigurationManager.AppSettings["ProduceCode"];
        //private static readonly string KioskMainFilePath = ConfigurationManager.AppSettings["KioskMainFilePath"];
        //private static readonly string MongoDbDataPath = ConfigurationManager.AppSettings["MongoDBDataPath"];
        //private static readonly string ReplaceKioskConfigFile = ConfigurationManager.AppSettings["ReplaceKioskConfigFile"];

        private static readonly string EmailFrom = ConfigurationManager.AppSettings["EmailFrom"];
        private static readonly string EmailFromPassword = ConfigurationManager.AppSettings["EmailFromPassword"];
        private static readonly string EmailTo = ConfigurationManager.AppSettings["EmailTo"];
        private static readonly string CcTo = ConfigurationManager.AppSettings["CcTo"];
        private static readonly string EmailHost = ConfigurationManager.AppSettings["EmailHost"];
        private static readonly string EmailPort = ConfigurationManager.AppSettings["EmailPort"];
        private static readonly bool EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);

        private static readonly string ApplicationConfigFilePath = ConfigurationManager.AppSettings["ApplicationConfigFilePath"];
        private static readonly string TestPlanConfigFilePath = ConfigurationManager.AppSettings["TestPlanConfigFilePath"];
        private static readonly string TestCaseConfigFilePath = ConfigurationManager.AppSettings["TestCaseConfigFilePath"];
        private static readonly string WebDriversFilePath = ConfigurationManager.AppSettings["WebDriversFilePath"];
        private static readonly string TempFilePath = ConfigurationManager.AppSettings["TempFilePath"];

        //private static readonly string InstallTimeout = ConfigurationManager.AppSettings["InstallTimeout"];
        //private static readonly string UninstallTimeout = ConfigurationManager.AppSettings["UninstallTimeout"];

        private static readonly int OperationDelay = Convert.ToInt32(ConfigurationManager.AppSettings["OperationDelay"]);
        private static readonly bool ContinueWhenError = Convert.ToBoolean(ConfigurationManager.AppSettings["ContinueWhenError"]);

        private static readonly bool RestartApplicationWhenError =
            Convert.ToBoolean(ConfigurationManager.AppSettings["RestartApplicationWhenError"]);
        
        public static void Test(string filename)
        {
            lock (SynchronizingObject)
            {
                string localInstallerFileName = Path.Combine(LocalAPKFolder, Path.GetFileName(filename));
                //1. copy installer file to local.
                CopyInstallFile(filename, localInstallerFileName);

                Test();
            }
        }

        public static void Test()
        {
            Log.Write("starting to executing test plan automatically.");
            foreach (string testplan in Directory.GetFiles(TestPlanConfigFilePath))
            {
                TestPlan tp = null;
                try
                {
                    tp = new TestPlan(testplan, ApplicationConfigFilePath,
       TestCaseConfigFilePath, WebDriversFilePath, TempFilePath, EmailFrom, EmailFromPassword,
       EmailTo, CcTo, EmailHost, int.Parse(EmailPort), EnableSsl, null);
                }
                catch (Exception ex)
                {
                    Log.Write("Test cannot be executed, the reason is: " + ex.Message);
                }
                tp.TestingProgressChange += tp_TestingProgressChange;
                if (RestartApplicationWhenError && ContinueWhenError)
                {
                    tp.TestCaseOnError = (cmd, isLast) =>
                    {
                        if (isLast)
                            return;
                        //DesktopAppCommand command = cmd as DesktopAppCommand;
                        //if (command != null)
                        //{
                        //    //RestoreDb();
                        //    command.Restart();
                        //}
                    };
                    tp.TestCaseOnError = (cmd, isLast) =>
                    {
                        if (isLast)
                            return;
                        //AndroidCommand command = cmd as AndroidCommand;
                        //if (command != null)
                        //{
                        //    command.Restart();
                        //}
                    };
                }
                try
                {
                    tp.Execute(OperationDelay, ContinueWhenError);
                }
                catch (Exception ex)
                {
                    Log.Write("There is errpr, information is ： {0}", ex.Message);
                }
            }
            Log.Write("aotumated testing completed.");
        }

        //private static void SetConfig()
        //{
        //    if (File.Exists(ReplaceKioskConfigFile))
        //    {
        //        Log.Write("starting to replay kiosk config file.");
        //        string configFilePath = KioskMainFilePath + ".config";
        //        XDocument doc = XDocument.Load(configFilePath);

        //        using (StreamReader sr = new StreamReader(ReplaceKioskConfigFile))
        //        {
        //            string line;
        //            while ((line = sr.ReadLine()) != null)
        //            {
        //                string key = line.Substring(0, line.IndexOf(':'));
        //                string value = line.Substring(line.IndexOf(':') + 1);

        //                doc.Descendants("add")
        //                    .First(
        //                        s =>
        //                            s.Attribute("key")
        //                                .Value.Equals(key, StringComparison.CurrentCultureIgnoreCase))
        //                    .SetAttributeValue("value", value);
        //            }
        //        }

        //        doc.Save(configFilePath);
        //        Log.Write("kiosk config file replaced.");
        //    }
        //}

        //public static void RestoreDb()
        //{
        //    //stop service
        //    Log.Write("starting to stop mongo db service.");
        //    try
        //    {
        //        GetService("MongoDB").Stop();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write("get error: {0}; continue next step", ex.Message);
        //    }
        //    Log.Write("mongo db service stoped.");

        //    //restore mongo db file
        //    Log.Write("starting to restore mongo db data file.");
        //    int retry = 5;
        //    while (retry > 0)
        //    {

        //        try
        //        {
        //            CopyDirectory(MongoDbDataPath + "_ren", MongoDbDataPath);
        //            break;
        //        }
        //        catch
        //        {
        //            retry--;
        //            Thread.Sleep(5000);
        //        }
        //    }
        //    Log.Write("mongo db data file restored.");

        //    //start mongo db service
        //    Log.Write("starting to start mongo db service.");
        //    try
        //    {
        //        GetService("MongoDB").Start();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write("get error: {0}; continue next step", ex.Message);
        //    }
        //    Log.Write("mongo db service started.");
        //}

        //private static void Install(string localInstallerFileName)
        //{
        //    if (!File.Exists(KioskMainFilePath))
        //    {
        //        Log.Write("starting to install new kiosk program.");

        //        KillMsiexec();

        //        //try to clean up program folder and install kiosk
        //        try
        //        {
        //            string dic = Path.GetDirectoryName(KioskMainFilePath);
        //            if (Directory.Exists(dic))
        //            {
        //                string exceptionFolder = Path.Combine(dic, "Exception");
        //                string logFolder = Path.Combine(dic, "Logs");
        //                string zipFile = Path.Combine(dic, "kiosklogs.zip");
        //                if(Directory.Exists(exceptionFolder))
        //                    Directory.Delete(exceptionFolder, true);
        //                if (Directory.Exists(logFolder))
        //                    Directory.Delete(logFolder, true);
        //                if (File.Exists(zipFile))
        //                    File.Delete(zipFile);
        //            }
        //        }
        //        finally
        //        {
        //            ExecuteCommand(string.Format(@"{0} /qn", localInstallerFileName));
        //        }
        //        //WaitingForExit("msiexec", InstallTimeout);

        //        int timeout;
        //        if (!int.TryParse(InstallTimeout, out timeout))
        //            timeout = 300;
        //        DateTime starttime = DateTime.Now;
        //        bool bResult = false;

        //        while ((DateTime.Now - starttime).TotalSeconds <= timeout)
        //        {
        //            if (!File.Exists(KioskMainFilePath) || !HasService("MongoDB") || GetService("MongoDB").Status != ServiceControllerStatus.Running)
        //            {
        //                Thread.Sleep(1000);
        //                continue;
        //            }
        //            bResult = true;
        //            break;
        //        }

        //        if (!bResult)
        //            throw new Exception(string.Format("install kiosk program failed: {0}", localInstallerFileName));

        //        KillMsiexec(true);

        //        Log.Write("kiosk program has been installed.");
        //    }
        //    else
        //    {
        //        throw new Exception("kiosk has been installed before try to install new version.");
        //    }
        //}

        //private static void BackupDb()
        //{
        //    if (Directory.Exists(MongoDbDataPath))
        //    {
        //        Log.Write("starting to backup mongo db file.");
        //        if (Directory.Exists(MongoDbDataPath + "_ren"))
        //            Directory.Delete(MongoDbDataPath + "_ren", true);
        //        Directory.Move(MongoDbDataPath, MongoDbDataPath + "_ren");
        //        Log.Write("mongo db file has been backed up.");
        //    }
        //}

        //private static void Uninstall()
        //{
        //    if (HasService("MongoDB") || File.Exists(KioskMainFilePath))
        //    {
        //        Log.Write("starting to uninstall local kiosk program.");

        //        KillMsiexec();

        //        ExecuteCommand(string.Format(@"C:\Windows\System32\msiexec.exe /x {0} /qn", ProduceCode));

        //        int timeout;
        //        if (!int.TryParse(UninstallTimeout, out timeout))
        //            timeout = 30;
        //        DateTime starttime = DateTime.Now;
        //        bool bResult = false;

        //        while ((DateTime.Now - starttime).TotalSeconds <= timeout)
        //        {
        //            if (HasService("MongoDB") || File.Exists(KioskMainFilePath))
        //            {
        //                Thread.Sleep(1000);
        //                continue;
        //            }
        //            bResult = true;
        //            break;
        //        }

        //        if (!bResult)
        //            throw new Exception("uninstall kiosk program failed.");

        //        KillMsiexec(true);

        //        Log.Write("local kiosk program has been uninstalled.");
        //    }
        //    else
        //    {
        //        Log.Write("checked local kiosk program was not installed.");
        //    }
        //}

        private static void KillMsiexec(bool waitfordelay = false)
        {
            if (waitfordelay)
            {
                Thread.Sleep(60 * 1000);
            }
        }

        static void tp_TestingProgressChange(int progress, TestPlan.ProgressChangeEvent e)
        {
            if (e.Status == TestPlan.TestingStatus.LoadingApplications &&
                e.CurrentProgress == 0)
            {
                Log.Write(new string('-', 30) + e.TestCase +" : "+e.CommandLine+ new string('-', 30));
                return;
            }
            Log.Write("{0} {1}/{2} {3} {4} {5}", e.Status, e.CurrentProgress, e.MaxProgress, e.TestCase, e.CommandLine, e.ErrorMessage);
        }

        private static void ExecuteCommand(string command)
        {
            using (Process cmd = new Process())
            {
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.RedirectStandardError = true;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.Start();
                cmd.StandardInput.WriteLine(command);
                cmd.StandardInput.WriteLine("exit");
                while (!cmd.HasExited)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private static bool HasService(string serviceName)
        {
            return ServiceController.GetServices().Any(s => s.ServiceName.Equals(serviceName, StringComparison.CurrentCultureIgnoreCase));
        }

        private static ServiceController GetService(string serviceName)
        {
            return ServiceController.GetServices().FirstOrDefault(s => s.ServiceName.Equals(serviceName, StringComparison.CurrentCultureIgnoreCase));
        }

        private static void CopyInstallFile(string filename, string localInstallerFileName)
        {
            Log.Write("starting to copy install file to local folder: {0}", LocalAPKFolder);
            File.Copy(filename, localInstallerFileName, true);
            Log.Write("install file has been copy to local folder: {0}", localInstallerFileName);
        }

        //private static void CopyDirectory(string src, string dest)
        //{
        //    if (Directory.Exists(src))
        //    {
        //        if (Directory.Exists(dest))
        //            Directory.Delete(dest, true);
        //        Directory.CreateDirectory(dest);
        //        foreach (string file in Directory.GetFiles(src, "*.*", SearchOption.AllDirectories))
        //        {
        //            string destFile = dest + file.Substring(src.Length);
        //            string destDic = Path.GetDirectoryName(destFile);
        //            if (!Directory.Exists(destDic))
        //            {
        //                Directory.CreateDirectory(destDic);
        //            }
        //            File.Copy(file, dest + file.Substring(src.Length), true);
        //        }
        //    }
        //}

        //public static string[] GetKioskLogs()
        //{
        //    string dfsFolder = Path.GetDirectoryName(KioskMainFilePath);
        //    string zipFilePath = Path.Combine(dfsFolder, "kiosklogs.zip");
        //    if (File.Exists(zipFilePath))
        //    {
        //        File.Delete(zipFilePath);
        //    }
        //    using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(zipFilePath))
        //    {
        //        zip.AddDirectory(Path.Combine(dfsFolder, "Exception"), "Exception");
        //        zip.AddDirectory(Path.Combine(dfsFolder, "Logs"), "Logs");
        //        zip.Save();
        //        return new[] {zip.Name};
        //    }
        //}
    }
}
