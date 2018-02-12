using AutomatedQA.CommonSources.Common;
using AutomatedQA.CommonSources.Controllers;
using AutomatedQA.CommonSources.Events;
//using AutomatedQA.CommonSources.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AutomatedQA.CommonSources.MainControls
{
    public class MainController
    {
        #region Fields

        #endregion

        #region Collections
        //private UtilityController _utilityController;
        private readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();
        private TestPlan.TestPlan _currentRunningPlan;
        #endregion

        #region Properties
        //private event EventHandler<ResetDesktopEventArgs> _resetDesktopEvent;
        //public event EventHandler<ResetDesktopEventArgs> ResetDesktopEvent
        //{
        //    add
        //    {
        //        this._resetDesktopEvent += value;
        //    }
        //    remove
        //    {
        //        this._resetDesktopEvent -= value;
        //    }

        //}

        
       
        #endregion

        #region Class Functionality
        public MainController()
        {
            // _utilityController = new UtilityController();
             
             AutoQAEventsAndArgs.InitializeEventsAndArgs();
        }

        ~MainController() // destructor
        {
            
        }

        public void SetRunningTestPlan(ref TestPlan.TestPlan plan)
        {
            _currentRunningPlan = plan;
        }                
              
        #endregion

        #region Helper Methods

        #endregion

        #region Delegate and EventFunctions
        public void _currentTestPlan_ResetDesktopEvent(object sender, EventArgs e)
        {
            ResetDesktopEventArgs args = e as ResetDesktopEventArgs;
            if (args != null)
            {
            }
            else
            {
                // exception  
                return;
            }
        }

        //public void _desktopProcessStart
        #endregion
               
    }
}
