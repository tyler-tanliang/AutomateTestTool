using AutomatedQA.CommonSources.Common;
using AutomatedQA.CommonSources.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;

namespace AutomatedQA.DesktopApp
{


    public class DesktopAppCommand : ICommand
    {

        public ICommandExecutor Executor { get; set; }

        public string[] SpecialParameters
        {
            get { return _specialParameters; }
            set { _specialParameters = value; }
        }

        private readonly AppElementCollection _app;
        //private Process _process;
        private string _currentRegionId;
        private string _currentRegionName;
        private bool _bStartFromTestingTool;
        private string[] _specialParameters;
        private Dictionary<string, string> _variables = null;

        public DesktopAppCommand(AppElementCollection app)
        {
            _app = app;
            _specialParameters = new[] {"-i"};
            Init();
            Executor = new DesktopAppCommandExecutor();
        }

        public void Init()
        {
            if (DesktopController.InitializeDesktop(_app))
            {
                _bStartFromTestingTool = true;
            }

            //_regions = new Dictionary<string, string>(_app.Regions.Count);
            foreach (var r in _app.Regions)
            {
                bool found = false;
                DateTime end = DateTime.Now.AddMinutes(20);
                while (end > DateTime.Now)
                {
                    AutomationElement win = AutomationElement.RootElement.FindFirst(TreeScope.Children, new AndCondition(
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window),
                        new PropertyCondition(AutomationElement.NameProperty, r.Name),
                        new PropertyCondition(AutomationElement.ProcessIdProperty, DesktopController.DesktopProcess.Id)
                        ));
                    if (win != null)
                    {
                        found = true;
                        if (_currentRegionId == null)
                        {
                            _currentRegionId = r.Id;
                            _currentRegionName = r.Name;
                        }
                        break;
                    }
                }
                if (!found)
                    throw new Exception(string.Format("Cannot find window {0}", r.Name));
            }
        }

        public void Dispose()
        {
            if (_bStartFromTestingTool)
            {
                Close();
            }
        }

        public void Close()
        {
            if (!DesktopController.DesktopProcess.HasExited)
            {
                DesktopController.CloseDesktop();
            }
        }
        public string GetVariable(string id)
        {
            string value = string.Empty;

            if (_variables.ContainsKey(id))
            {
                value = _variables[id];
            }
            return value;
        }
        public void Input(string id, string value, params string[] arguments)
        {
            try
            {

                bool inputed = false;
                string voucherContainer = string.Empty;
                for (int i = 0; i < 5; i++)
                {
                    AutomationElement element = GetElement(id);
                    if (element.Current.IsEnabled && element.GetSupportedPatterns().Contains(ValuePattern.Pattern))
                    {
                        if (value == "stack")
                        {
                            voucherContainer = DataVault.StoredVoucher;
                            if (voucherContainer == "")
                            {
                                throw new Exception(string.Format("Input fail: {0} fails because Voucher Stack is Empty.", id));
                            }
                            else
                            {
                                ((ValuePattern)GetElement(id).GetCurrentPattern(ValuePattern.Pattern)).SetValue(voucherContainer);
                                inputed = true;
                            }
                        }
                        else
                        {
                            ((ValuePattern)GetElement(id).GetCurrentPattern(ValuePattern.Pattern)).SetValue(value);
                            inputed = true;
                        }
                        break;
                    }
                    Thread.Sleep(1000);
                }
                if (!inputed)
                    throw new Exception(string.Format("Input fail: {0} cannot be edited.", id));
            }
            catch
            {
                if (arguments.Contains("-i"))
                    return;
                throw;
            }
        }

        public void Click(string id, params string[] arguments)
        {
            try
            {
                bool clicked = false;
                AutomationElement element = null;
                for (int i = 0; i < 5; i++)
                {
                    element = GetElement(id);
                    if (element.Current.IsEnabled && element.GetSupportedPatterns().Contains(InvokePattern.Pattern))
                    {
                        ((InvokePattern)element.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
                        clicked = true;
                        break;
                    }
                    if (element.Current.IsEnabled && element.GetSupportedPatterns().Contains(ExpandCollapsePattern.Pattern))
                    {
                        ((ExpandCollapsePattern)element.GetCurrentPattern(ExpandCollapsePattern.Pattern)).Expand();
                        ((ExpandCollapsePattern)element.GetCurrentPattern(ExpandCollapsePattern.Pattern)).Collapse();
                        clicked = true;
                        break;
                    }
                    if (element.Current.IsEnabled && !element.Current.IsOffscreen)
                    {
                        WinApi.ClickLeftMouse(element);
                        clicked = true;
                        break;
                    }
                    Thread.Sleep(1000);
                }
                if (!clicked)
                    throw new Exception(string.Format("Click fail: {0} cannot be clicked.", id));
            }
            catch
            {
                if (arguments.Contains("-i"))
                    return;
                throw;
            }
        }

        public void ClickIfExists(string id, params string[] arguments)
        {
            try
            {
                AutomationElement element = GetElementNullable(id);
                if (element != null && element.Current.IsEnabled)
                {
                    this.Click(id, arguments);
                }
                else
                {
                    //  TODO
                    //  not exception but needs to be reported somewhere
                    //  throw new Exception(string.Format("element isnot exist: {0}", id));
                }
            }
            catch
            {
                if (arguments.Contains("-i"))
                    return;
                throw;
            }
        }

        public bool IfExists(string id, params string[] arguments)
        {
            bool responseValue = false;
            try
            {
                if (GetElement(id) != null)
                {
                    responseValue = true;
                }

            }
            catch
            {
                if (!arguments.Contains("-i"))
                    throw;
            }
            return responseValue;
        }

        public void ClickIfOnScreen(string id, params string[] arguments)
        {
            try
            {
                AutomationElement element = GetElementNullable(id);
                if (element != null && element.Current.IsEnabled && !element.Current.IsOffscreen)
                {
                    this.Click(id, arguments);
                }
                else
                {
                    //  TODO
                    //  not exception but needs to be reported somewhere
                    //  throw new Exception(string.Format("element isnot exist: {0}", id));
                }
            }
            catch
            {
                if (arguments.Contains("-i"))
                    return;
                throw;
            }
        }
        public void Assert(string id, string name, string value, params string[] arguments)
        {
            switch (name)
            {
                case "name":
                    if (!GetElement(id).Current.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase))
                        throw new Exception(string.Format("Check fail: {0} not equal to \"{1}\"", id, value));
                    break;
            }
        }

        public void Check(string id, string value, params string[] arguments)
        {
            Assert(id, "name", value, arguments);
        }

        public void Exist(string id, params string[] arguments)
        {
            try
            {
                if (GetElement(id) == null)
                    throw new Exception(string.Format("element does not exist: {0}", id));
            }
            catch
            {
                if (arguments.Contains("-i"))
                    return;
                throw;
            }
        }

        public void Select(string id, int index, params string[] arguments)
        {
            try
            {
                bool selected = false;
                for (int i = 0; i < 5; i++)
                {
                    AutomationElement element;
                    if (index == 0)
                    {
                        element = GetElement(id);
                    }
                    else
                    {
                        AutomationElement parent;
                        parent =
                            GetElement(id);
                        AutomationElementCollection children = parent
                                .FindAll(TreeScope.Children,
                                    new PropertyCondition(AutomationElement.ProcessIdProperty, DesktopController.DesktopProcess.Id));
                        if (children.Count >= index)
                            element = children[index - 1];
                        else
                            continue;
                    }
                    if (element.Current.IsEnabled && element.GetSupportedPatterns().Contains(SelectionItemPattern.Pattern))
                    {
                        ((SelectionItemPattern)element.GetCurrentPattern(SelectionItemPattern.Pattern)).Select();
                        selected = true;
                        break;
                    }
                    Thread.Sleep(1000);
                }
                if (!selected)
                    throw new Exception(string.Format("Select fail: {0} cannot be selected.", id));
            }
            catch
            {
                if (arguments.Contains("-i"))
                    return;
                throw;
            }
        }

        public void Select(string id, string text, params string[] arguments)
        {
            throw new NotImplementedException();
        }

        public void Wait(string id, int timeOutSeconds = 60, params string[] arguments)
        {
            try
            {
                DateTime end = DateTime.Now.AddSeconds(timeOutSeconds);
                bool hasElement = false;
                while (end > DateTime.Now)
                {
                    try
                    {
                        Exist(id);
                        hasElement = true;
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(100);
                    }
                }
                if (!hasElement)
                    throw new Exception(string.Format("Waitfor {0} timeout ({1}s)", id, timeOutSeconds));
            }
            catch
            {
                if (arguments.Contains("-i"))
                    return;
                throw;
            }
        }

        public void Sleep(int secondsValue)
        {
            Thread.Sleep(secondsValue * 1000);
        }

        public void Switch(string regionId, params string[] arguments)
        {
            try
            {
                GetWindow(_app[regionId].Name).SetFocus();
                _currentRegionId = regionId;
                _currentRegionName = _app[regionId].Name;
            }
            catch
            {
                if (arguments.Contains("-i"))
                    return;
                throw;
            }
        }
        public void SwitchPage(int numberPage, params string[] arguments)
        {
            throw new NotImplementedException();
        }
        public void Swipe(string StartId,params string[] arguments)
        {
            throw new NotImplementedException();
        }

        public void LongPress(string Id, int duration, params string[] arguments)
        {
            throw new NotImplementedException();
        }

        public void Execute(string commandline)
        {
            Executor.Execute(this, commandline);
        }

        public IntPtr[] GetHandles()
        {
            return _app.Regions.Select(r => new IntPtr(GetWindow(r.Name).Current.NativeWindowHandle)).ToArray();
        }

        private AutomationElement CurrentWindow
        {
            get { return GetWindow(_currentRegionName); }
        }

        private string GetValueWithJS(string javascriptParameter)
        {
            return string.Empty;
        }

        private AutomationElement GetWindow(string name)
        {
            AutomationElement element = AutomationElement.RootElement.FindFirst(TreeScope.Children,
                new AndCondition(
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window)
                    , new PropertyCondition(AutomationElement.NameProperty, name)
                    , new PropertyCondition(AutomationElement.ProcessIdProperty, DesktopController.DesktopProcess.Id)
                    ));
            //maybe app restart, the process id was changed. need to refrush process id.
            if (element == null)
            {
                DesktopController.InitializeDesktop(_app);
                element =
                    AutomationElement.RootElement.FindFirst(TreeScope.Children,
                        new AndCondition(
                            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window)
                            , new PropertyCondition(AutomationElement.NameProperty, name)
                            , new PropertyCondition(AutomationElement.ProcessIdProperty, DesktopController.DesktopProcess.Id)
                            ));
            }
            //still cannot get element
            if (element == null)
            {
                throw new Exception(string.Format("Cannot found window: {0}", name));
            }
            return element;
        }

        private AutomationElement GetElement(string id)
        {
            try
            {
                AutomationElement element;
                string signature = _app[_currentRegionId].Elements.Find(e => e.Id == id).Signature;
                string[] byParams = signature.Split(':');
                switch (byParams[0].ToLower())
                {
                    case "id":
                        element = CurrentWindow.FindFirst(TreeScope.Subtree,
                            new PropertyCondition(AutomationElement.AutomationIdProperty, byParams[1]));
                        break;
                    case "name":
                        element = CurrentWindow.FindFirst(TreeScope.Subtree,
                            new PropertyCondition(AutomationElement.NameProperty, byParams[1]));
                        break;
                    case "textbox":
                        element =
                            CurrentWindow.FindAll(TreeScope.Subtree,
                                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit))[
                                    int.Parse(byParams[1]) - 1];
                        break;
                    case "button":
                        element =
                            CurrentWindow.FindAll(TreeScope.Subtree,
                                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button))[
                                    int.Parse(byParams[1]) - 1];
                        break;
                    default:
                        throw new Exception();
                }
                if (element == null)
                    throw new Exception();
                return element;
            }
            catch
            {
                throw new Exception(string.Format("Unknown element {0}", id));
            }
        }


        private AutomationElement GetElementNullable(string id)
        {
            AutomationElement element;
            string signature = _app[_currentRegionId].Elements.Find(e => e.Id == id).Signature;
            string[] byParams = signature.Split(':');
            switch (byParams[0].ToLower())
            {
                case "id":
                    element = CurrentWindow.FindFirst(TreeScope.Subtree,
                        new PropertyCondition(AutomationElement.AutomationIdProperty, byParams[1]));
                    break;
                case "name":
                    element = CurrentWindow.FindFirst(TreeScope.Subtree,
                        new PropertyCondition(AutomationElement.NameProperty, byParams[1]));
                    break;
                case "textbox":
                    element =
                        CurrentWindow.FindAll(TreeScope.Subtree,
                            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit))[
                                int.Parse(byParams[1]) - 1];
                    break;
                case "button":
                    element =
                        CurrentWindow.FindAll(TreeScope.Subtree,
                            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button))[
                                int.Parse(byParams[1]) - 1];
                    break;
                default:
                    element = null;
                    break;
            }

            return element;
        }

        public void Restart()
        {
            Close();
            Thread.Sleep(5000);
            Init();
        }

        public void GetRegionElements()
        {
            string message = string.Empty;
            Condition conditions = new AndCondition(
              new PropertyCondition(AutomationElement.IsEnabledProperty, true),
              new PropertyCondition(AutomationElement.ControlTypeProperty,
                  ControlType.Button)
              );

            AutomationElementCollection element = CurrentWindow.FindAll(TreeScope.Descendants, conditions);

            foreach (AutomationElement item in element)
            {
                message += string.Format("name: {0} Id: {1} class: {2}\n", item.Current.Name, item.Current.AutomationId, item.Current.ClassName);

            }

            MessageBox.Show(message);
        }

        public void DragAndDrop(string StartId, params string[] arguments)
        {
            throw new NotImplementedException();
        }
        public void Scroll(params string[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}
