//using AutomatedQA.DesktopApp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace AutomatedQA.CommonSources.Events
{
    #region Delegates
    public delegate void UpdateUI();
    #endregion

     

    public static class AutoQAEventsAndArgs
    {
        #region collection and fields
        private static EventHandler<NotifyUIEventArgs> UIEventReference;
        #endregion 
                
        private static event EventHandler<NotifyUIEventArgs> _notifyUIofChange;
        public static event EventHandler<NotifyUIEventArgs> NotifyUIofChange
        {
            add
            {
                _notifyUIofChange = value;
                UIEventReference = AutoQAEventsAndArgs._notifyUIofChange;
            }
            remove
            {
                _notifyUIofChange = value;
            }
        }

        public static void InitializeEventsAndArgs()
        {
            UIEventReference = AutoQAEventsAndArgs._notifyUIofChange;

        }
                            

        public static void RaiseUINotification(object obj,string controlName,string message)
        {
            NotifyUIEventArgs nuiea = new NotifyUIEventArgs();
            nuiea.name = controlName;
            nuiea.message = message;
            UIEventReference(obj, nuiea);
        }
        
        public static void RaiseUINotification(object obj, Dictionary<string,string> parameters)
        {
            NotifyUIEventArgs nuiea = new NotifyUIEventArgs();
            nuiea.messageCollection = parameters;
            UIEventReference(obj, nuiea);
        }
                
    }

    public class NotifyUIEventArgs : EventArgs
    {
        public string type;
        public string name;
        public string message;
        public Dictionary<string, string> messageCollection;

    }

    public class ResetDesktopEventArgs : EventArgs
    {
        //private readonly DesktopAppCommand _command;

        //public ResetDesktopEventArgs(DesktopAppCommand command)
        //{
        //    _command = command;
        //}

        //public DesktopAppCommand Command
        //{
        //    get { return _command; }
        //}

    }
}
