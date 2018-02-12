using AutomatedQA.Android;
using AutomatedQA.CommonSources.Common;
using AutomatedQA.CommonSources.Events;
//using AutomatedQA.DesktopApp;
using AutomatedQA.Web;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;

namespace AutomatedQA.TestPlan
{
    public class TestPlan
    {
        
        private static string _applicationConfigPath;
        private static string _tempFilePath;
        private static string _webDriversPath;
        private static string _emailFrom;
        private static string _emailPassword;
        private static string _emailTo;
        private static string _ccTo;
        private static string _emailHost;
        private static int _emailPort;
        private static bool _enableSsl;
        private Func<string[]> _attachmentGetter;
        private static string _testcaseConfigPath;

        #region Properties
        private event EventHandler<ResetDesktopEventArgs> _resetDesktopEvent;
        public event EventHandler<ResetDesktopEventArgs> ResetDesktopEvent
        {
            add
            {
                this._resetDesktopEvent += value;
            }
            remove
            {
                this._resetDesktopEvent -= value;
            }

        }
        #endregion


        public TestPlan(string testPlanFileName, string applicationConfigPath, string testCaseConfigPath, string webDriversPath, string tempFilePath,
            string emailFrom, string emailPassword, string emailTo, string ccTo, string emailHost, int emailPort, bool enableSsl, Func<string[]> attachmentGetter = null)
        {
            _applicationConfigPath = applicationConfigPath;
            _testcaseConfigPath = testCaseConfigPath;
            _tempFilePath = tempFilePath;
            _webDriversPath = webDriversPath;
            _emailFrom = emailFrom;
            _emailPassword = emailPassword;
            _emailTo = emailTo;
            _ccTo = ccTo;
            _emailHost = emailHost;
            _emailPort = emailPort;
            _enableSsl = enableSsl;
            _attachmentGetter = attachmentGetter;

            if (!File.Exists(testPlanFileName))
            {
                throw new Exception("Test Plan is not found.");
            }

            _name = Path.GetFileNameWithoutExtension(testPlanFileName);
            TestCases = new List<string>();

            _testCaseStatus = new DataTable();
            _testCaseStatus.Columns.Add("Index");
            _testCaseStatus.Columns.Add("Test Case");
            _testCaseStatus.Columns.Add("Description");
            _testCaseStatus.Columns.Add("Status");
            _testCaseStatus.Columns.Add("Result");
            _testCaseStatus.Columns.Add("Exception");
            _testCaseStatus.Columns.Add("Image");

            using (StreamReader sr = new StreamReader(testPlanFileName, System.Text.Encoding.Default))
            {
                string line = sr.ReadLine();
                if (line == null)
                    throw new Exception("errors in test plan file: " + testPlanFileName);

                try
                {
                    foreach (string parameter in line.Split(';'))
                    {
                        string environmentApp = parameter.Split(',')[0];
                        string environmentType = parameter.Split(',')[1];
                        string paraNameAPP = environmentApp.Split(':')[0];
                        string paraValueAPP = environmentApp.Split(':')[1];
                        string paraNameType = environmentType.Split(':')[0];
                        string paraValueType = environmentType.Split(':')[1];
                        TestEnvironment.Add(paraValueAPP, paraValueType);
                        //switch (paraName)
                        //{
                        //    case "Application":
                        //        TestEnvironment.Add(paraValue, null);                                
                        //        break;
                        //    case "BrowserType":
                        //        TestEnvironment.va
                        //        BrowserType = paraValue;
                        //        break;
                        //}
                    }
                    int i = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("--") || string.IsNullOrEmpty(line))
                            continue;
                        i++;
                        string testcasefilename = Path.Combine(testCaseConfigPath, line);
                        if (!File.Exists(testcasefilename))
                            throw new Exception(testcasefilename + " is not existed in testing folder");
                        TestCases.Add(testcasefilename);

                        DataRow dr = _testCaseStatus.NewRow();
                        dr["Index"] = i;
                        dr["Test Case"] = testcasefilename;
                        dr["Status"] = "Ready";

                        using (StreamReader srTestCase = new StreamReader(testcasefilename))
                        {
                            string testcaseLine = srTestCase.ReadLine();
                            if (!string.IsNullOrEmpty(testcaseLine) && testcaseLine.StartsWith("--"))
                            {
                                dr["Description"] = testcaseLine.Substring(2);
                            }
                            //check all lines of testcases, make the inner testcase in test case is correct.
                            while ((testcaseLine = srTestCase.ReadLine()) != null)
                            {
                                if (testcaseLine.ToLower().StartsWith("innertestcase"))
                                {
                                    List<string> parameters = CommonHelper.SplitCommand(testcaseLine, " ");
                                    string innerTestcasefilename = Path.Combine(testCaseConfigPath, parameters[1]);
                                    if (!File.Exists(innerTestcasefilename))
                                        throw new Exception("Test case \"" + line + "\" in the plan has inner testcase " + innerTestcasefilename + " , but " + innerTestcasefilename + " is not existed in testing folder");
                                }
                            }
                        }

                        _testCaseStatus.Rows.Add(dr);
                    }
                }

                catch (Exception ex)
                {
                    throw ex;
                }
            }       

        }

        

        private readonly string _name;

        public Dictionary<string, string> TestEnvironment = new Dictionary<string, string>();
        public string  CurrentApplication { get; set; }
        public string CurrentBrowserType { get; set; }
        public List<string> TestCases { get; set; }
        private Dictionary<string, ICommand> _command=new Dictionary<string, ICommand>();
        private readonly DataTable _testCaseStatus;
        public DataTable TestCaseStatus
        {
            get { return _testCaseStatus; }
        }

        #region ProgressReport
        public delegate void TestingProgressHandle(int progress, ProgressChangeEvent e);
        public event TestingProgressHandle TestingProgressChange;
        public Action<ICommand,bool> TestCaseOnError = null;

        [System.Flags]
        public enum TestingStatus
        {
            LoadingApplications = 1,
            ExecutingTestCase = 2,
            TestCaseSuccess = 4,
            TestCaseFail = 8,
            SendingReport = 16,
            Completed = 32,
            ExecutingInnerTestCase = 64
        }

        public class ProgressChangeEvent : EventArgs
        {
            public TestingStatus Status { get; set; }
            public int CurrentProgress { get; set; }
            public int MaxProgress { get; set; }
            public string CommandLine { get; set; }
            public string ErrorMessage { get; set; }
            public string ErrorScreenshotPath { get; set; }
            public string TestCase { get; set; }
        }

        public void ReportProgress(TestingStatus status, int currentProgress, int maxProgress, string commandLine,
            string errorMessgae, string errorScreenshotPath, string testCase)
        {
            if (!string.IsNullOrEmpty(testCase))
            {
                DataRow dr = _testCaseStatus.Rows[currentProgress];

                dr["Status"] = status.ToString();
                dr["Test Case"] = testCase;

                if (status == TestingStatus.LoadingApplications)
                {
                    dr["Result"] = commandLine;
                    dr["Exception"] = errorMessgae;
                    dr["Image"] = errorScreenshotPath;
                }
                else if (status != TestingStatus.ExecutingTestCase && status!=TestingStatus.ExecutingInnerTestCase)
                {
                    dr["Result"] = string.IsNullOrEmpty(errorMessgae) ? "Passed" : "Failed";
                    dr["Exception"] = errorMessgae;
                    dr["Image"] = errorScreenshotPath;
                }
                else
                {
                    dr["Result"] = commandLine;
                }
            }

            if (TestingProgressChange != null)
            {
                TestingProgressChange(currentProgress,
                    new ProgressChangeEvent
                    {
                        Status = status,
                        CurrentProgress = status.ToString().Contains("TestCase") && !status.ToString().Contains("Inner") ? currentProgress + 1 : currentProgress,
                        MaxProgress = maxProgress,
                        CommandLine = commandLine,
                        ErrorMessage = errorMessgae,
                        ErrorScreenshotPath = errorScreenshotPath,
                        TestCase = testCase
                    });
            }
        }

        public void ReportProgress(TestingStatus status, int currentProgress, int maxProgress, string testCase)
        {
            ReportProgress(status, currentProgress, maxProgress, string.Empty, string.Empty, string.Empty, testCase);
        }
        public void ReportProgress(TestingStatus status, int currentProgress, int maxProgress, string commandLine, string testCase)
        {
            ReportProgress(status, currentProgress, maxProgress, commandLine, string.Empty, string.Empty, testCase);
        }

        #endregion ProgressReport

        public void Execute(int delay = 1000, bool continueWhenError = true,bool sendEmail=true)
        {
            ICommand currentCommand = null;
            //create new temp image folder
            _tempFilePath = Path.Combine(_tempFilePath, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
            if (!Directory.Exists(_tempFilePath))
                Directory.CreateDirectory(_tempFilePath);

            ReportProgress(TestingStatus.LoadingApplications, 0, 1, "Start Preparing environment", _name);
            //ReportProgress(TestingStatus.LoadingApplications, 0, 1, _name);
            //Load app config
            List<AppElementCollection> appElementCollection = Directory.GetFiles(_applicationConfigPath, "*.xml", SearchOption.AllDirectories).Select(AppElementCollection.LoadFromConfigFile).ToList();
            appElementCollection.ForEach(app => app.Regions.ForEach(region => region.Elements.ForEach(element => element.Signature = element.Signature.Replace("\\r", "\r").Replace("\\n", "\n"))));

            //Load app
            foreach(var t in TestEnvironment)
            {
                AppElementCollection appCollection = appElementCollection.FirstOrDefault(a => a.Name == t.Key);
                if (appCollection.Type.Equals("web", StringComparison.CurrentCultureIgnoreCase))
                {
                    currentCommand = new WebCommand(t.Value, _webDriversPath, appCollection);
                }
                else if (appCollection.Type.Equals("android", StringComparison.CurrentCultureIgnoreCase))
                {
                    currentCommand = new AndroidCommand("android", appCollection);
                }
                else if (appCollection.Type.Equals("ios", StringComparison.CurrentCultureIgnoreCase))
                {
                    //_command = new IOSCommand("ios", appCollection);
                }
                else
                {
                    //_command = new DesktopAppCommand(appCollection);
                }
                _command.Add(t.Key, currentCommand);
                CurrentBrowserType = t.Value;
            }
            //AppElementCollection appCollection = appElementCollection.FirstOrDefault(a => a.Name == Application);

            
            ReportProgress(TestingStatus.LoadingApplications, 0, 1, "End Preparing environment", _name);
            //ReportProgress(TestingStatus.LoadingApplications, 1, 1, string.Empty);   

            #region Execute Test Cases
            //Execute test cases

            
            string regionName = string.Empty;
            for (int i = 0; i < TestCases.Count; i++)
            {
                string strCommand = null;
                int errorBmpCount = 0;
                using (StreamReader sr = new StreamReader(TestCases[i]))
                {
                    int j = 0;
                    try
                    {

                        while ((strCommand = sr.ReadLine()) != null)
                        {
                            j++;
                            //ingore the comments of the testcase
                            if (strCommand.StartsWith("--"))
                                continue;

                            if (strCommand.ToLower().StartsWith("SwitchToApp:".ToLower()))
                            {
                                string appName = strCommand.Substring(12).Trim();
                                if (_command.ContainsKey(appName))
                                {
                                    currentCommand = _command[appName];
                                    CurrentApplication = appName;
                                    CurrentBrowserType = TestEnvironment[appName];
                                }
                                continue;
                            }
                            //if test case include inner testcase
                            if (strCommand.ToLower().StartsWith("innertestcase"))
                            {
                                List<string> parameters = CommonHelper.SplitCommand(strCommand, " ");
                                string innerTestcasefilename = Path.Combine(_testcaseConfigPath, parameters[1]);
                                string innerCommand=string.Empty;
                                using (StreamReader testcaseSr = new StreamReader(innerTestcasefilename))
                                {
                                    while ((innerCommand = testcaseSr.ReadLine()) != null)
                                    {
                                        //ingore the comments of the testcase
                                        if (innerCommand.StartsWith("--"))
                                            continue;
                                        //Escape character
                                        innerCommand = System.Text.RegularExpressions.Regex.Unescape(innerCommand);
                                        
                                         ReportProgress(TestingStatus.ExecutingInnerTestCase, i, TestCases.Count, innerCommand, TestCases[i] + "{" + parameters[1] + "}");
                                        currentCommand.Execute(innerCommand);
                                        Thread.Sleep(delay);
                                    }
                                }
                                continue;                                
                            }
                            //Escape character if not inner test case
                            else
                            {
                                strCommand = System.Text.RegularExpressions.Regex.Unescape(strCommand);
                            }
                            ReportProgress(TestingStatus.ExecutingTestCase, i, TestCases.Count, strCommand, TestCases[i]);
                            currentCommand.Execute(strCommand);
                            Thread.Sleep(delay);
                        }
                        ReportProgress(TestingStatus.TestCaseSuccess, i, TestCases.Count, string.Empty, TestCases[i]);
                    }
                    catch (Exception ex)
                    {
                        string errBmp = string.Empty;
                        if (!CurrentBrowserType.ToLower().Equals("ios")&& !CurrentBrowserType.ToLower().Equals("android"))
                        {
                            errorBmpCount++;
                            OpenQA.Selenium.IWebDriver ExecutingDriver = currentCommand.Executor.Driver;
                            string oldBmpFile = string.Format("{0}\\{1}){2}({3})-old.jpg", _tempFilePath, i + 1, Path.GetFileNameWithoutExtension(TestCases[i]), errorBmpCount);
                            string bmpFile = string.Format("{0}\\{1}){2}({3}).jpg", _tempFilePath, i + 1, Path.GetFileNameWithoutExtension(TestCases[i]), errorBmpCount);
                            ((ITakesScreenshot)ExecutingDriver).GetScreenshot().SaveAsFile(oldBmpFile, ScreenshotImageFormat.Jpeg);
                            errBmp = oldBmpFile;
                            Image oldPic = Image.FromFile(errBmp);
                            EncoderParameters ep = new EncoderParameters();
                            long qy;
                            try
                            {
                                string s = TestingDossier.ScreenShotQuality;
                                long.TryParse(s, out qy);
                            }
                            catch
                            {
                                qy = 100;
                            }
                            EncoderParameter eParam = new EncoderParameter(Encoder.Quality, qy);
                            ep.Param[0] = eParam;
                            ImageCodecInfo jpegIcIinfo = ImageCodecInfo.GetImageEncoders().First(e => e.FormatDescription.Equals("JPEG"));

                            Bitmap newPic = new Bitmap(oldPic, (int)(oldPic.Width * 0.5), (int)(oldPic.Height * 0.5));
                            newPic.Save(bmpFile, ImageFormat.Jpeg);
                            errBmp = bmpFile + ";";
                        }
                        /*

                        List<IntPtr> handles = new List<IntPtr>();
                        handles.AddRange(_command.GetHandles());
                        
                        Bitmap[] b = WinApi.GetWindowPic(handles.ToArray());
                        string errBmp = string.Empty;
                        foreach (Bitmap pic in b)
                        {
                                                        
                            errorBmpCount++;
                            string bmpFile = string.Format("{0}\\{1}){2}({3}).jpg", _tempFilePath, i + 1, Path.GetFileNameWithoutExtension(TestCases[i]), errorBmpCount);
                            EncoderParameters ep = new EncoderParameters();
                            long qy;
                            try
                            {
                                string s = TestingDossier.ScreenShotQuality;
                                long.TryParse(s, out qy);
                            }
                            catch
                            {
                                qy = 100;
                            }
                            EncoderParameter eParam = new EncoderParameter(Encoder.Quality, qy);
                            ep.Param[0] = eParam;
                            ImageCodecInfo jpegIcIinfo = ImageCodecInfo.GetImageEncoders().First(e => e.FormatDescription.Equals("JPEG"));
                            pic.Save(bmpFile, jpegIcIinfo, ep);
                            errBmp += bmpFile + ";";

                        }
                        */

                        //Get the screen of the android or IOS application if error
                        if (CurrentBrowserType.ToLower().Equals("android", StringComparison.CurrentCultureIgnoreCase) || CurrentBrowserType.ToLower().Equals("ios", StringComparison.CurrentCultureIgnoreCase))
                        {
                            errorBmpCount++;
                            OpenQA.Selenium.Appium.AppiumDriver<OpenQA.Selenium.IWebElement> ExecutingDriver = (OpenQA.Selenium.Appium.AppiumDriver<OpenQA.Selenium.IWebElement>)currentCommand.Executor.Driver;
                            string oldBmpFile = string.Format("{0}\\{1}){2}({3})-old.jpg", _tempFilePath, i + 1, Path.GetFileNameWithoutExtension(TestCases[i]), errorBmpCount);
                            string bmpFile = string.Format("{0}\\{1}){2}({3}).jpg", _tempFilePath, i + 1, Path.GetFileNameWithoutExtension(TestCases[i]), errorBmpCount);
                            ExecutingDriver.GetScreenshot().SaveAsFile(oldBmpFile, ScreenshotImageFormat.Jpeg);
                            errBmp = oldBmpFile;
                            Image oldPic = Image.FromFile(errBmp);
                            EncoderParameters ep = new EncoderParameters();
                            long qy;
                            try
                            {
                                string s = TestingDossier.ScreenShotQuality;
                                long.TryParse(s, out qy);
                            }
                            catch
                            {
                                qy = 100;
                            }
                            EncoderParameter eParam = new EncoderParameter(Encoder.Quality, qy);
                            ep.Param[0] = eParam;
                            ImageCodecInfo jpegIcIinfo = ImageCodecInfo.GetImageEncoders().First(e => e.FormatDescription.Equals("JPEG"));

                            Bitmap newPic = new Bitmap(oldPic, (int)(oldPic.Width * 0.5), (int)(oldPic.Height * 0.5));
                            newPic.Save(bmpFile,ImageFormat.Jpeg);
                            errBmp = bmpFile + ";";
                        }                        

                        ReportProgress(TestingStatus.TestCaseFail, i, TestCases.Count, strCommand,
                            string.Format("error at line \"{0}: {1}\" message: {2}", j, strCommand, ex.Message),
                            errBmp, TestCases[i]);

                        if (TestCaseOnError != null)
                        {
                            TestCaseOnError(currentCommand, i == TestCases.Count - 1);
                        }
                        else
                        {
                            //RaiseResetDesktop(_command);
                        }

                        if (!continueWhenError)
                        {
                            break;
                        }
                    }
                }
            }

            foreach (var command in _command)
            {
                command.Value.Dispose();
            }
            //currentCommand.Dispose();


            ReportProgress(TestingStatus.SendingReport, 1, 1, string.Empty);

            #endregion

            string[] attachments = null;

            //attachments = _attachmentGetter();
            if (sendEmail)
            {
                try
                {
                    SendReport.SendReportByEmail(
                        string.Format("Automated Testing Report: TP: {0} -- {1:yyyyMMdd}", _name, DateTime.Now),
                        _emailFrom, _emailPassword, _emailTo, _ccTo, _emailHost, _emailPort, _enableSsl, _tempFilePath,
                        _testCaseStatus, attachments);
                    ReportProgress(TestingStatus.Completed, 1, 1, string.Empty);
                }
                catch (Exception ex)
                {
                    ReportProgress(TestingStatus.Completed, 1, 1, string.Empty, ex.Message, string.Empty, string.Empty);
                    throw ex;
                }
            }
        }

        private void RaiseResetDesktop(Dictionary<string, ICommand> _commands)
        {
            
            foreach (var command in _commands)
            {
                //if (command.Value is DesktopAppCommand)
                //{
                //    ResetDesktopEventArgs ea = new ResetDesktopEventArgs((DesktopAppCommand)command.Value);

                //    var resetHandler = _resetDesktopEvent;
                //    if (resetHandler == null)
                //        return;

                //    resetHandler(this, ea);
                //    break;
                //}
            }
        }
    }
}
