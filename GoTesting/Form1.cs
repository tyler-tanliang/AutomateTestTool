using AutomatedQA;
using AutomatedQA.CommonSources.Common;
using AutomatedQA.CommonSources.Controllers;
using AutomatedQA.CommonSources.Events;
using AutomatedQA.CommonSources.MainControls;
//using AutomatedQA.DesktopApp;
using AutomatedQA.TestPlan;
using AutomatedQA.Web;
using GoTesting.ToolConfig;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SearchOption = System.IO.SearchOption;
//using AutomatedQA.Android;

namespace GoTesting
{
    public partial class Form1 : Form
    {
        #region Configuration settings
        private readonly string _applicationConfigFilePath =
            ConfigurationManager.AppSettings["ApplicationConfigFilePath"];

        private readonly string _testPlanConfigFilePath =
            ConfigurationManager.AppSettings["TestPlanConfigFilePath"];

        private readonly string _testCaseConfigFilePath =
            ConfigurationManager.AppSettings["TestCaseConfigFilePath"];

        private readonly string _webDriversFilePath =
            ConfigurationManager.AppSettings["WebDriversFilePath"];

        private readonly string _tempFilePath =
            ConfigurationManager.AppSettings["TempFilePath"];

        private readonly string _emailFrom =
            ConfigurationManager.AppSettings["EmailFrom"];

        private readonly string _emailFromPassword =
            ConfigurationManager.AppSettings["EmailFromPassword"];

        private readonly string _emailTo =
            ConfigurationManager.AppSettings["EmailTo"];

        private readonly string _ccTo =
            ConfigurationManager.AppSettings["CcTo"];

        private readonly string _emailHost =
            ConfigurationManager.AppSettings["EmailHost"];

        private readonly string _emailPort =
            ConfigurationManager.AppSettings["EmailPort"];

        private readonly bool EnableSsl =
            Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);

        private static readonly bool RestartApplicationWhenError =
            Convert.ToBoolean(ConfigurationManager.AppSettings["RestartApplicationWhenError"]);

        private static readonly bool ContinueWhenError = Convert.ToBoolean(ConfigurationManager.AppSettings["ContinueWhenError"]);

        private static readonly string LogFilePath = ConfigurationManager.AppSettings["LogFilePath"];
        #endregion

        #region class Functions
        public Form1()
        {
            InitializeComponent();
            InitilizeTestingDossier();
            InitilizeMainController();
        }

        #endregion
        
        #region fields

        private readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();
        private int _delay;
        private MainController _mainController;
        private readonly object syncObject = new object();

        private TestPlan _currentTestPlan;
        private TestPlan CurrentTestPlan
        {
            get { return _currentTestPlan; }
            set
            {
                _currentTestPlan = value;
                _mainController.SetRunningTestPlan(ref value);
            }
        }
        #endregion fields

        #region Properties
        private string _loadTestCommandField;
        public string LoadTestCommandField
        {
            get
            {
                return _loadTestCommandField;
            }
            set
            {
                _loadTestCommandField = value;               
            }
        }
        

        #endregion
        
        #region events
        private void Form1_Load(object sender, EventArgs e)
        {
            cbDelay.Text = @"1000";
            btnExecuteTestPlan.Enabled = false;
            dataGridView1.ReadOnly = true;
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
            AutoQAEventsAndArgs.NotifyUIofChange += _form_UpdateUI;
            LoadTestPlan();
            Icon = new Icon("ico/go.ico");
        }

        private void cbTestPlan_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnExecuteTestPlan.Enabled = cbTestPlan.Text.Length > 0;
        }

        private void btnExecuteTestPlan_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(cbDelay.Text, out _delay) || _delay < 0)
            {
                MessageBox.Show(@"Delay is an invalid value.", @"Error", MessageBoxButtons.OK);
                return;
            }
            groupBox1.Enabled = false;
            dataGridView1.DataSource = null;
            _delay = int.Parse(cbDelay.Text);
            _backgroundWorker.RunWorkerAsync(cbTestPlan.Text);
        }       

        protected override void OnClosed(EventArgs e)
        {
            _backgroundWorker.Dispose();
            base.OnClosed(e);
        }

        #endregion

        #region methods

        private void LoadTestPlan()
        {
            foreach (string testplan in Directory.GetFiles(_testPlanConfigFilePath, "*.txt", SearchOption.AllDirectories))
            {
                cbTestPlan.Items.Add(testplan);
            }
        }

        private void InitilizeTestingDossier()
        {
            TestingDossier.InitializeDossier(
                applicationConfigFilePath: ConfigurationManager.AppSettings["ApplicationConfigFilePath"],
                testPlanConfigFilePath: ConfigurationManager.AppSettings["TestPlanConfigFilePath"],
                testCaseConfigFilePath: ConfigurationManager.AppSettings["TestCaseConfigFilePath"],                
                webDriversFilePath: ConfigurationManager.AppSettings["WebDriversFilePath"],
                tempFilePath: ConfigurationManager.AppSettings["TempFilePath"],
                appFileName: ConfigurationManager.AppSettings["AppFileName"],
                emailFrom: ConfigurationManager.AppSettings["EmailFrom"],
                emailFromPassword: ConfigurationManager.AppSettings["EmailFromPassword"],
                emailTo: ConfigurationManager.AppSettings["EmailTo"],
                ccTo: ConfigurationManager.AppSettings["CcTo"],
                emailHost: ConfigurationManager.AppSettings["EmailHost"],
                emailPort: ConfigurationManager.AppSettings["EmailPort"],
                enableSsl: Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]),
                restartApplicationWhenError: Convert.ToBoolean(ConfigurationManager.AppSettings["RestartApplicationWhenError"]),
                logFilePath: ConfigurationManager.AppSettings["LogFilePath"],
                screenShotQuality: ConfigurationManager.AppSettings["ScreenShotQuality"],
                continueWhenError: Convert.ToBoolean(ConfigurationManager.AppSettings["ContinueWhenError"]),
                androidDeviceName: ConfigurationManager.AppSettings["AndroidDeviceName"],
                androidPlatformVersion: ConfigurationManager.AppSettings["AndroidPlatformVersion"],
                androidAppPackage: ConfigurationManager.AppSettings["AndroidAppPackage"],
                androidAppActivity: ConfigurationManager.AppSettings["AndroidAppActivity"],      
                androidAppWaitActivity: ConfigurationManager.AppSettings["AndroidAppWaitActivity"],
                sourceAPKFolder: ConfigurationManager.AppSettings["SourceAPKFolder"],  
                aPPFileName: ConfigurationManager.AppSettings["APKFileName"], 
                localAPPFolder: ConfigurationManager.AppSettings["LocalAPKFolder"],
                driverServerIP: ConfigurationManager.AppSettings["DriverServerIP"],
                driverServerPort: ConfigurationManager.AppSettings["DriverServerPort"]
            );
        }

        private void InitilizeMainController()
        {
            _mainController = new MainController();
        }

        
       
        public void _form_UpdateUI(object sender, EventArgs e)
        {
            lock (syncObject)
            {
                NotifyUIEventArgs args = e as NotifyUIEventArgs;
                if (args != null)
                {
                    if (args.messageCollection == null)
                    {
                        Control[] changedControl = this.Controls.Find(args.name, true);
                        if (changedControl.Count() > 0)
                        {
                            TextBox boxContainer = (TextBox)changedControl[0];
                            boxContainer.BeginInvoke((MethodInvoker)delegate { boxContainer.Text = args.message; });
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<string, string> messageItem in args.messageCollection)
                        {
                            Control[] changedControl = this.Controls.Find(messageItem.Key, true);
                            string test = messageItem.Key;
                            if (changedControl.Count() > 0)
                            {
                                TextBox boxContainer = (TextBox)changedControl[0];
                                boxContainer.BeginInvoke((MethodInvoker)delegate { boxContainer.Text = messageItem.Value; });
                            }
                        }
                    }
                }
                else
                {
                    // exception  
                    return;
                }
            }

        }
        #endregion methods

        #region backgroundworker

        private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Log.LogFileName = Path.Combine(LogFilePath, DateTime.Now.ToString("yyyyMMdd-H.lo\\g"));
            try
            {
                _currentTestPlan = new TestPlan(e.Argument.ToString(), _applicationConfigFilePath, _testCaseConfigFilePath,
                _webDriversFilePath, _tempFilePath, _emailFrom, _emailFromPassword, _emailTo, _ccTo, _emailHost,
                int.Parse(_emailPort), EnableSsl, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Test cannot be executed, the reason is: " + ex.Message);
            }
            _currentTestPlan.TestingProgressChange += _currentTestPlan_TestingProgressChange;
            _currentTestPlan.ResetDesktopEvent += _mainController._currentTestPlan_ResetDesktopEvent;

            if (RestartApplicationWhenError && ContinueWhenError)
            {
                //_currentTestPlan.TestCaseOnError = (cmd,isLast) =>
                //{
                //    if (isLast)
                //        return;
                //    //DesktopAppCommand command = cmd as DesktopAppCommand;
                //    if (command != null)
                //    {
                //        //AutoTesting.RestoreDb();
                //        command.Restart();
                //    }
                //};
                //_currentTestPlan.TestCaseOnError = (cmd, isLast) =>
                //{
                //    if (isLast)
                //        return;
                //    //AndroidCommand command = cmd as AndroidCommand;
                //    if (command != null)
                //    {
                //        command.Restart();
                //    }
                //};
                _currentTestPlan.TestCaseOnError = (cmd, isLast) =>
                {
                    if (isLast)
                        return;
                    //IOSCommand command = cmd as IOSCommand;
                    //if (command != null)
                    //{
                    //    command.Restart();
                    //}
                };
            }
            try
            {
                _currentTestPlan.Execute(_delay, TestingDossier.ContinueWhenError,SendEmail.Checked);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), @"Unhandled Exception");
            }
        }

        private void _currentTestPlan_TestingProgressChange(int progress, TestPlan.ProgressChangeEvent e)
        {
            _backgroundWorker.ReportProgress(progress, e);
        }



        private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TestPlan.ProgressChangeEvent state = e.UserState as TestPlan.ProgressChangeEvent;
            if (state == null)
                return;
            tsProgress.Visible = true;
            dataGridView1.DataSource = _currentTestPlan.TestCaseStatus;
            tsProgress.Value = state.CurrentProgress;
            tsProgress.Maximum = state.MaxProgress;
            tsStatus.Text = state.Status.ToString();

            if (state.Status == AutomatedQA.TestPlan.TestPlan.TestingStatus.LoadingApplications)
                dataGridView1.Rows[e.ProgressPercentage].DefaultCellStyle.BackColor = Color.LightBlue;
            if (state.Status == AutomatedQA.TestPlan.TestPlan.TestingStatus.ExecutingTestCase
                || state.Status == AutomatedQA.TestPlan.TestPlan.TestingStatus.ExecutingInnerTestCase)
                dataGridView1.Rows[e.ProgressPercentage].DefaultCellStyle.BackColor = Color.Yellow;
            if (state.Status == AutomatedQA.TestPlan.TestPlan.TestingStatus.TestCaseSuccess)
                dataGridView1.Rows[e.ProgressPercentage].DefaultCellStyle.BackColor = Color.Green;
            if (state.Status == AutomatedQA.TestPlan.TestPlan.TestingStatus.TestCaseFail)
                dataGridView1.Rows[e.ProgressPercentage].DefaultCellStyle.BackColor = Color.Red;
        }

        private void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            groupBox1.Enabled = true;
            tsStatus.Text = @"Ready";
            tsProgress.Visible = false;
            GC.Collect();
        }








        #endregion backgroundworker
    }
}
