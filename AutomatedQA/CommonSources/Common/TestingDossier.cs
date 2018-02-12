using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedQA.CommonSources.Common
{
    public static class TestingDossier
    {
        public static string ApplicationConfigFilePath { private set; get; }
        public static string TestPlanConfigFilePath { private set; get; }
        public static string TestCaseConfigFilePath { private set; get; }
        public static string WebDriversFilePath { private set; get; }
        public static string TempFilePath { private set; get; }
        public static string EmailFrom { private set; get; }
        public static string EmailFromPassword { private set; get; }
        public static string EmailTo { private set; get; }
        public static string CcTo { private set; get; }
        public static string EmailHost { private set; get; }
        public static string EmailPort { private set; get; }
        public static bool EnableSsl { private set; get; }
        public static bool RestartApplicationWhenError { private set; get; }
        public static string LogFilePath { private set; get; }
        public static string ScreenShotQuality { private set; get; }
        public static string AppFileName { private set; get; }
        public static bool ContinueWhenError { private set; get; }
        public static List<AppElementCollection> appElementCollection;
        public static Dictionary <string,List<Element>> ErrorList;

        public static string AndroidDeviceName { private set; get; }
        public static string AndroidPlatformVersion { private set; get; }
        public static string AndroidAppPackage { private set; get; }
        public static string AndroidAppActivity  { private set; get; }
        public static string AndroidAppWaitActivity { private set; get; }
        public static string SourceAPKFolder { private set; get; }
        public static string APPFileName { private set; get; }
        public static string LocalAPPFolder { private set; get; }

        public static string DriverServerIP { private set; get; }
        public static string DriverServerPort { private set; get; }

        public static void InitializeDossier(
            string applicationConfigFilePath,
            string testPlanConfigFilePath,
            string testCaseConfigFilePath,
            string webDriversFilePath,
            string tempFilePath,
            string appFileName,
            string emailFrom,
            string emailFromPassword,
            string emailTo,
            string ccTo,
            string emailHost,
            string emailPort,
            bool enableSsl,
            bool restartApplicationWhenError,
            string logFilePath,
            string screenShotQuality,
            bool continueWhenError,
            string androidDeviceName,
            string androidPlatformVersion,
            string androidAppPackage,
            string androidAppActivity,
            string androidAppWaitActivity,
            string sourceAPKFolder,
            string aPPFileName,
            string localAPPFolder,
            string driverServerIP,
            string driverServerPort
            )
        {
            ApplicationConfigFilePath = applicationConfigFilePath;
            TestPlanConfigFilePath = testPlanConfigFilePath;
            TestCaseConfigFilePath = testCaseConfigFilePath;
            WebDriversFilePath = webDriversFilePath;
            TempFilePath = tempFilePath;
            AppFileName = appFileName;
            EmailFrom = emailFrom;
            EmailFromPassword = emailFromPassword;
            EmailTo = emailTo;
            CcTo = ccTo;
            EmailHost = emailHost;
            EmailPort = emailPort;
            EnableSsl = enableSsl;
            RestartApplicationWhenError = restartApplicationWhenError;
            LogFilePath = logFilePath;
            ScreenShotQuality = screenShotQuality;
            ContinueWhenError = continueWhenError;
            AndroidDeviceName = androidDeviceName;
            AndroidPlatformVersion = androidPlatformVersion;
            AndroidAppPackage = androidAppPackage;
            AndroidAppActivity = androidAppActivity;
            AndroidAppWaitActivity = androidAppWaitActivity;
            SourceAPKFolder = sourceAPKFolder;
            APPFileName = aPPFileName;
            LocalAPPFolder = localAPPFolder;
            DriverServerIP =driverServerIP;
            DriverServerPort = driverServerPort;
            appElementCollection = Directory.GetFiles(ApplicationConfigFilePath, "*.xml", SearchOption.AllDirectories).Select(AppElementCollection.LoadFromConfigFile).ToList();

            InitializeErrorList();

        }

        private static void InitializeErrorList()
        {
            ErrorList = new Dictionary<string, List<Element>>();
                      
            foreach ( AppElementCollection appItem in appElementCollection)
            {
                foreach (Region regionItem in appItem.Regions)
                {
                    List<Element> elementList = regionItem.Elements.Where(e => e.Signature.StartsWith("Error")).ToList();
                    if (elementList.Count > 0)
                    {
                        foreach (Element elementItem in elementList)
                        {
                            // removes prefix from Signature
                            elementItem.Signature = elementItem.Signature.Split(':').Last();
                        }
                        ErrorList.Add(regionItem.Id, elementList);
                    }
                }
            }
           
        }
    }
}
