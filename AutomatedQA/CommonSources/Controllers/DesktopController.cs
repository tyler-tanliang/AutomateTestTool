using AutomatedQA.CommonSources.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutomatedQA.CommonSources.Common;

namespace AutomatedQA.CommonSources.Controllers
{
    public static class DesktopController
    {
        #region events and background stuff
        //private static BackgroundWorker _desktopMemoryMonitor;
                
        #endregion
        
        #region properties and collections        

        private static Process[] processes;
        public static Process DesktopProcess { get; set; }

        #endregion

        public static bool InitializeDesktop(AppElementCollection _app)
        {
            bool returnStatus = false;
            processes = Process.GetProcessesByName(_app.Name);
            if (processes.Length == 1)
            {
                DesktopProcess = processes[0];
            }
            else
            {
                DesktopProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = _app.Path,
                        WorkingDirectory = Path.GetDirectoryName(_app.Path),
                        UseShellExecute = false
                    }
                };
                DesktopProcess.Start();

                

                returnStatus = true;
            }          

            return returnStatus;
        }
        
        public static void CloseDesktop()
        {            
            //_desktopMemoryMonitor.Dispose();
            DesktopProcess.Kill();
            DesktopProcess.Dispose();
        }

    }
}
