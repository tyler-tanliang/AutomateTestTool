using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutomatedQA.CommonSources.Common;

namespace GoTesting.ToolConfig
{
    public partial class TestingController : Form
    {
        public TestingController()
        {
            InitializeComponent();
            InitilizeTestingDossier();
        }

        readonly NotifyIcon _notifyicon = new NotifyIcon();
        readonly ContextMenuStrip _notifyContextMenu = new ContextMenuStrip();
        private readonly Icon _ico = new Icon("ico/go.ico");
        private bool _allowexit = true;

        private void btnStart_Click(object sender, EventArgs e)
        {
            GoTestingTester.Start();
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            WindowState = FormWindowState.Minimized;
            _notifyicon.Text = @"started";
            _allowexit = false;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            GoTestingTester.Stop();
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            _notifyicon.Text = @"stoped";
            _allowexit = true;
        }

        private void TestingController_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_allowexit)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
                return;
            }
            if (GoTestingTester.Timer != null)
                GoTestingTester.Timer.Dispose();
        }

        private void TestingController_Load(object sender, EventArgs e)
        {
            _notifyicon.MouseClick += _notifyicon_MouseClick;
            _notifyicon.Text = @"stoped";
            SizeChanged += TestingController_SizeChanged;
            ToolStripItem exit = new ToolStripButton("Exit");
            exit.Click += exit_Click;
            _notifyContextMenu.Items.Add(exit);
            _notifyicon.ContextMenuStrip = _notifyContextMenu;
            this.Icon = _ico;
        }

        void exit_Click(object sender, EventArgs e)
        {
            _allowexit = true;
            _notifyicon.Dispose();
            Close();
        }

        void _notifyicon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _notifyContextMenu.Show();
            }
            else
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Normal;
                    ShowInTaskbar = true;
                    _notifyicon.Visible = false;
                    Show();
                    Activate();
                }
            }
        }
        
        private void TestingController_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                _notifyicon.Icon = _ico;
                ShowInTaskbar = false;
                _notifyicon.Visible = true;
                Hide();
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
    }
}
