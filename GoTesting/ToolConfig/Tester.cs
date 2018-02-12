using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace GoTesting.ToolConfig
{
    class GoTestingTester
    {
        //        <add key="SourceAPKFolder" value="\\BUILD01\AgentBuildDrop\Dev"/>
        //<add key="APKFileName" value="fishingsys-release.apk"/>
        //<add key="LocalAPKFolder" value="C:\Users\Tyler\Desktop\yuyeAPK\"/>
        //private static readonly string InstallerFileName = System.Configuration.ConfigurationManager.AppSettings["InstallerFileName"];
        //private static readonly string RootPath = System.Configuration.ConfigurationManager.AppSettings["RootPath"];
        private static readonly string TestPlanFileFolder = System.Configuration.ConfigurationManager.AppSettings["TestPlanConfigFilePath"];
        private static readonly string APKFileName = System.Configuration.ConfigurationManager.AppSettings["APKFileName"];
        private static readonly string SourceAPKFolder = System.Configuration.ConfigurationManager.AppSettings["SourceAPKFolder"];
        private static readonly string LocalAPKFolder = System.Configuration.ConfigurationManager.AppSettings["LocalAPKFolder"];
        private static readonly string LogFilePath = System.Configuration.ConfigurationManager.AppSettings["LogFilePath"];
        private static readonly int[] NewVersionDetectHours = System.Configuration.ConfigurationManager.AppSettings["NewVersionDetectHours"].Split(',').Select(int.Parse).ToArray();
        //private static bool _reInstallDfs;
        public static Timer Timer;
        private static bool _isRunning;

        public static void Start()
        {
            Timer = new Timer(Execute, null, 0, 10000);
        }

        private static void Execute(object arg)
        {
            if (_isRunning)
                return;
            _isRunning = true;
            try
            {
                //If there is only web test plan, no need to check the release folder
                bool onlyWebTest = true;
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string TestPlanFilePath = baseDirectory + TestPlanFileFolder;
                if (Directory.Exists(TestPlanFilePath))
                {
                    foreach (string testplanFileName in Directory.GetFiles(TestPlanFilePath))
                    {
                        using (StreamReader sr = new StreamReader(testplanFileName, System.Text.Encoding.Default))
                        {
                            string line = sr.ReadLine();
                            if (line == null)
                                throw new Exception("errors in test plan file: " + testplanFileName);

                            try
                            {
                                foreach (string parameter in line.Split(';'))
                                {
                                    string paraName = parameter.Split(':')[0];
                                    string paraValue = parameter.Split(':')[1];
                                    if (paraName.ToLower().Equals("browsertype") && (paraValue.Trim().ToLower().Equals("ios")|| paraValue.Trim().ToLower().Equals("android")))
                                    {
                                        onlyWebTest = false;
                                        break;
                                    }
                                    
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Errors in test plan file: " + testplanFileName + ". information is: " + ex.Message);
                            }
                            if (!onlyWebTest)
                            {
                                break;
                            }
                        }
                    }
                }

                Log.LogFileName = Path.Combine(LogFilePath, DateTime.Now.ToString("yyyyMMdd-H.lo\\g"));

                if (NewVersionDetectHours.Contains(DateTime.Now.Hour)&&!File.Exists(Log.LogFileName))
                {
                    if (!onlyWebTest)
                    {
                        var dailyPackage =
                                Directory.GetDirectories(SourceAPKFolder)
                                    .Where(
                                        d =>
                                            d.Contains(DateTime.Now.ToString("yyyyMMdd")) &&
                                            Directory.GetFiles(d, APKFileName, SearchOption.AllDirectories).Length > 0)
                                    .Select(d => Directory.GetFiles(d, APKFileName, SearchOption.AllDirectories)[0])
                                    .OrderByDescending(d => d)
                                    .FirstOrDefault();
                        if (dailyPackage != null)
                        {
                            Log.Write(new string('=', 80));
                            Log.Write("found new version: {0}", dailyPackage);
                            AutoTesting.Test(dailyPackage);
                        }
                        else
                        {
                            Log.Write("dont have new version on {0}", DateTime.Now.ToString("yyyyMMdd"));
                        }
                    }
                    if(onlyWebTest)
                    {
                        Log.Write(new string('=', 80));
                        Log.Write("Start to Executed Scheduled task, start at: {0}", DateTime.Now);
                        AutoTesting.Test();
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Log.Write(new string('*', 80));
                Log.Write("UnKnownException: {0}", ex);
                Log.Write(new string('*', 80));
            }
            finally
            {
                _isRunning = false;
            }
        }

        public static void Stop()
        {
            if (Timer != null)
                Timer.Dispose();
        }
    }
}
