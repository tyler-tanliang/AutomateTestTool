using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using AutomatedQA.CommonSources.Common;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;


namespace AutomatedQA.Android
{
    public class AndroidCommand : ICommand
    {
        public ICommandExecutor Executor { get; set; }

        public string[] SpecialParameters
        {
            get { return _specialParameters; }
            set { _specialParameters = value; }
        }

        private AndroidDriver<IWebElement> _driver;
        private IJavaScriptExecutor _javaScript;
        private readonly AppElementCollection _appElements;
        private readonly string _name;
        private List<string> _currentRegion;
        private string[] _specialParameters;
        private Dictionary<string, string> _variables = null;

        public AndroidCommand(string name, AppElementCollection app)
        {
            Executor = new AndroidCommandExecutor();
            _name = name;
            _appElements = app;
            _specialParameters = new[] {"-i","-r"};
            //Init the application at first time.
            Init("FirstInit");
            _currentRegion = new List<string>();
            _currentRegion.Add("Master");
        }

        private void Init(string type)
        {
            switch (_name)
            {
                case "android":
                    string apkFile = TestingDossier.LocalAPPFolder + TestingDossier.APPFileName;
                    DesiredCapabilities Capabilities = new DesiredCapabilities();
                    Capabilities.SetCapability(MobileCapabilityType.DeviceName, TestingDossier.AndroidDeviceName);                    
                    if (System.IO.File.Exists(apkFile))
                    {
                        Capabilities.SetCapability(MobileCapabilityType.App, apkFile);
                    }
                    else
                    {
                        Capabilities.SetCapability(AndroidMobileCapabilityType.AppPackage, TestingDossier.AndroidAppPackage);   
                        Capabilities.SetCapability(AndroidMobileCapabilityType.AppActivity, TestingDossier.AndroidAppActivity);
                    }
  
                    //Must init twice when application file is not null.
                    //first time is remove all data of the application, install new version.
                    //Second time, init the application with config noreset=true, then the application will restore the login information

                    if (type.Equals("FirstInit") &&
                        System.IO.File.Exists(apkFile))
                    {
                        for (int initTimes = 1; initTimes <= 2; initTimes++)
                        {
                            if (initTimes == 1)
                            {
                                //will uninstall the app then install it again.
                                Capabilities.SetCapability("fullReset", true);
                            }
                            else
                            {
                                //will keep the login and other information
                                Capabilities.SetCapability("fullReset", false);
                                Capabilities.SetCapability("noReset", true);
                            }
                            _driver = new AndroidDriver<IWebElement>(new Uri("http://127.0.0.1:4723/wd/hub"), Capabilities);
                            //first time is remove all data of the application.
                            if (initTimes == 1)
                            {
                                _driver.Dispose();
                                _driver = null;
                            }
                            else
                            {
                                //_driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));
                                _javaScript = (IJavaScriptExecutor)_driver;
                                //set the driver for screen shot
                                Executor.Driver = _driver;
                            }

                        }
                    }
                    else
                    {
                        _driver = new AndroidDriver<IWebElement>(new Uri("http://127.0.0.1:4723/wd/hub"), Capabilities);
                        _javaScript = (IJavaScriptExecutor)_driver;
                        //set the driver for screen shot
                        Executor.Driver = _driver;
                    }

                    //Restart the application if test case error, make sure next test case can continue. but not remove the data of the application, need store the login and other information

                    //if first init application, do not remove all data when there is not application file. 
                    //if (type.Equals("RestartApp") ||
                    //    (type.Equals("FirstInit") && !System.IO.File.Exists(apkFile)))
                    //{
                    //    Capabilities.SetCapability("appWaitActivity", TestingDossier.AndroidAppWaitActivity);
                    //    //will keep the login and other information
                    //    Capabilities.SetCapability("fullReset", false);
                    //    Capabilities.SetCapability("noReset", true);
                    //    _driver = new AndroidDriver<IWebElement>(new Uri("http://127.0.0.1:4723/wd/hub"), Capabilities);
                    //    _driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));
                    //    _javaScript = (IJavaScriptExecutor)_driver;
                    //    //set the driver for screen shot
                    //    Executor.Driver = _driver;
                    //}
                    break;

                default:
                    throw new ArgumentException("Unknown driver: " + _name);
            }
        }

        private IWebElement GetElement(string id)
        {
            IWebElement element = null;
            string[] index = null;
            if (id.Contains('[') && id.Contains(']'))
            {
                string[] idParts = id.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                id = idParts[0];
                index = idParts.Skip(1).ToArray();
            }
            string signature = GetSignature(id);
            string[] byParams = new string[2];
            int pos = signature.IndexOf(':');
            byParams[0] = signature.Substring(0, pos);
            byParams[1] = signature.Substring(pos + 1);

            //Get all contexts of the driver,try to locate the elemnt in different context;
            IList<string> driverContexts = _driver.Contexts;
            

            int triedTimes = 0;
            foreach (string context in driverContexts)
            {
                triedTimes++;
                _driver.Context = context;
                //_driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));
                _javaScript = (IJavaScriptExecutor)_driver;

                
                try
                {
                    #region js, but not implement rightnow for android in appium
                    if (byParams[0].Equals("js", StringComparison.CurrentCultureIgnoreCase))
                    {
                        //1. try to get element by js directly
                        object elementObj = _javaScript.ExecuteScript(byParams[1]);

                        if (elementObj == null)
                            //2. try to return element by js
                            elementObj = _javaScript.ExecuteScript("return " + byParams[1]);
                        if (elementObj == null)
                        {
                            //3. try to get element from collections by js
                            string indexString = "[" + string.Join("][", index) + "]";
                            if (byParams[1].Contains("return"))
                                elementObj = _javaScript.ExecuteScript(string.Format("{0}{1};", byParams[1], indexString));
                            else
                                elementObj = _javaScript.ExecuteScript(string.Format("return {0}{1};", byParams[1], indexString));
                        }
                        if (elementObj == null)
                        {
                            throw new Exception(string.Format("Cannot found element {0}", id));
                        }

                        //if target is a collections, to get the one matched index
                        if (elementObj is System.Collections.IList)
                        {
                            elementObj = index.Aggregate(elementObj, (current, i) => ((System.Collections.IList)current)[int.Parse(i)]);
                        }

                        if (elementObj is IWebElement)
                            element = elementObj as IWebElement;
                        //else
                        //    element = new TextWebElement(elementObj.ToString());
                    }
                #endregion
                    //other locate function but javascript.
                    else
                    {                        
                        By by=null;
                        if(byParams[0].Equals("AccessibilityId", StringComparison.CurrentCultureIgnoreCase))
                        {
                            by=new ByAccessibilityId(byParams[1].ToString());
                        }
                        else
                        {
                            by =
                            typeof(By).InvokeMember(byParams[0],
                                BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, null,
                                new object[] { byParams[1] }) as By;
                        }

                        if (index != null)
                        {
                            bool optionIsIndex = false;
                            int optionIndex = -1;
                            optionIsIndex = int.TryParse(index[0], out optionIndex);
                            if (!optionIsIndex)
                            {
                                if (index[0].StartsWith("\"") && index[0].EndsWith("\""))
                                {
                                    index[0] = index[0].Substring(1, index[0].Length - 2);
                                }
                            }
                            IList<IWebElement> foundList = _driver.FindElements(by);
                            //check the found element is list view
                            if (foundList.Count == 1 && foundList[0].TagName.ToLower().Contains("listview"))
                            {
                                IWebElement e = foundList[0];
                                string tagname = e.TagName;
                                string resource_id = e.GetAttribute("resourceId");
                                string xpath = string.Empty;
                                //if option is index, just search child, otherwise descendant.
                                if (optionIsIndex)
                                {
                                    xpath = string.Format("//{0}[@resource-id='{1}']/child::*", tagname, resource_id);
                                }
                                else
                                {
                                    xpath = string.Format("//{0}[@resource-id='{1}']/descendant::*", tagname, resource_id);
                                }
                                //get all children of the listview
                                IList<IWebElement> childElementList = _driver.FindElements(By.XPath(xpath));
                                if (childElementList.Count == 0)
                                {
                                    throw new OpenQA.Selenium.NoSuchElementException("There is not options for the element: " + id);
                                }
                                if (optionIndex >= childElementList.Count)
                                {
                                    throw new OpenQA.Selenium.NoSuchElementException("There is not option " + optionIndex + " in the element: " + id);
                                }
                                //check the option is index or content.
                                if (optionIsIndex)
                                {
                                    element = childElementList[optionIndex];
                                }
                                else
                                {
                                    element = childElementList.First(item => CommonHelper.IsMatchRegex(index[0], item.Text));
                                }
                                if (element == null)
                                {
                                    throw new OpenQA.Selenium.NoSuchElementException("There is not option " + index[0] + " in the element: " + id);
                                }

                            }
                            //if the result is collection, just get the item in the collection
                            else
                            {
                                //check the option is index or content.
                                if (optionIsIndex)
                                {
                                    element = foundList[optionIndex];
                                }
                                else
                                {
                                    element = foundList.First(item => CommonHelper.IsMatchRegex(index[0], item.Text));
                                }
                            }
                        }
                        //if there is [#], will return the element directly.
                        else
                        {
                            element = _driver.FindElement(by);
                        }
                         
                    }
                }
                catch
                {
                    if (triedTimes == driverContexts.Count)
                    {
                        throw new Exception(string.Format("Cannot found element {0}", id));
                    }
                }

                //If found the element in one context, quit the foreach
                if (element != null)
                {
                    break;
                }
            }
            return element;
        }

        private string GetSignature(string id)
        {
            try
            {
                var elements = _appElements.Regions.Where(r => _currentRegion.Contains(r.Id) || r.Id == "Master").SelectMany(r => r.Elements);
                string signature = elements.FirstOrDefault(e => e.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase)).Signature;
                return signature;
            }
            catch
            {
                throw new Exception(string.Format("Please check config file, there isn't an element named '{0}'.", id));
            }
        }
        private string GetValueWithJS(string javascriptParameter)
        {
            string valueOfJSGet = string.Empty;
            try
            {
                valueOfJSGet = (string)_javaScript.ExecuteScript(javascriptParameter.Substring(3));
            }
            catch (Exception ex)
            {
                throw new Exception("Java Script is error when get value with js, details is: " + ex.Message);
            }
            return valueOfJSGet;
        }


        public void Dispose()
        {
            _driver.Dispose();
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

                AndroidElement element = (AndroidElement)GetElement(id);
                element.Click();
                element.Clear();
                element.SendKeys(value);
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
                IWebElement element = GetElement(id);
                element.Click();
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
            try
            {
                IWebElement element = null;
                string resultText = null;
                if (!string.IsNullOrEmpty(id))
                    element = GetElement(id);
                bool matched = false;
                switch (name.ToLower())
                {
                    case "url":
                        resultText = _driver.Url;
                        break;
                    case "text":
                        if (element.TagName.ToLower() == "input")
                        {
                            switch (element.GetAttribute("type").ToLower())
                            {
                                case "text":
                                    resultText = element.GetAttribute("value");
                                    break;
                                case "radio":
                                case "checkbox":
                                    resultText = element.Selected.ToString();
                                    break;
                                default:
                                    resultText = element.GetAttribute("value");
                                    break;
                            }
                        }
                        else if (element.TagName.ToLower() == "select")
                        {
                            SelectElement selectElement = new SelectElement(element);
                            resultText = selectElement.SelectedOption.Text;
                        }
                        else
                        {
                            resultText = element.Text;
                        }
                        break;
                    default:
                        matched = element.GetAttribute(name).Equals(value, StringComparison.CurrentCultureIgnoreCase);
                        break;
                }

                //Compare the value between expected and actual with regex or other regular.
                matched = CommonHelper.IsMatchRegex(value, resultText);
                
                //sometimes when check a value is empty, on web page the element will be hidden, not clean the value/text
                if (element != null && !matched && string.IsNullOrEmpty(value) && !element.Displayed)
                    matched = true;
                if (!matched)
                    throw new Exception("Check failed: actual value of " + id + " is " + resultText);
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
            throw new NotImplementedException();
        }

        public bool IfExists(string id, params string[] arguments)
        {
            throw new NotImplementedException();
        }

        public void Exist(string id, params string[] arguments)
        {
            try
            {
                GetElement(id);
            }
            catch
            {
                if (arguments.Contains("-i"))
                    return;
                throw;
            }
        }

        public void Select(string id, string option, params string[] arguments)
        {
            try
            {
                IWebElement element = GetElement(id);
                element.Click();
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
            throw new NotImplementedException("No need in android testing");
        }

        public void Wait(string id, int timeOutSeconds, params string[] arguments)
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
                        continue;
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

        public void SwitchPage(int pageNumber, params string[] arguments)
        {
            //Need implement
            
            //bool matched = false;
            //Region region = _appElements[regionId];
            //if (region != null)
            //{
            //    matched = true;

            //}
            //else
            //{
            //    throw new Exception(string.Format("Region {0} is not exist.", regionId));
            //}
            //if (matched)
            //{
            //    _currentRegion = new List<string> { region.Id };
            //    if (region.ReferRegionList != null)
            //    {
            //        _currentRegion.AddRange(region.ReferRegionList);
            //    }
            //}
            

        }

        //move the screen from element to another.
        public void Swipe(string StartId, params string[] arguments)
        {
            try
            {
                if (arguments != null && 1 == arguments.Length)
                {
                    IWebElement endElement = GetElement(arguments[0]);
                    IWebElement startElement = GetElement(StartId);

                    int xEnd = endElement.Location.X;
                    int yEnd = endElement.Location.Y;

                    int xStart = startElement.Location.X;
                    int yStart = startElement.Location.Y;

                    TouchAction action = new TouchAction(_driver);
                    action.Press(xStart, yStart).MoveTo(xEnd - xStart, yEnd - yStart).Perform();
                    //_driver.Swipe(xStart, yStart, xEnd, yEnd, 1000);
                }
                else if (arguments != null && arguments.Length > 1)
                {
                    IWebElement startElement = GetElement(StartId);
                    Point startPoint = new Point(startElement.Location.X, startElement.Location.Y);
                    IList<Point> swipePoints = new List<Point>();
                    //IList<IWebElement> swipeElements = new List<IWebElement>();
                    foreach (string s in arguments)
                    {
                        IWebElement element = GetElement(s);
                        swipePoints.Add(new Point(element.Location.X, element.Location.Y));
                    }
                    TouchAction action = new TouchAction(_driver);
                    action.Press(startPoint.X, startPoint.Y);
                    for (int i = 0; i < swipePoints.Count; i++)
                    {
                        if (i == 0)
                        {
                            action.MoveTo(swipePoints[i].X - startPoint.X, swipePoints[i].Y - startPoint.Y);
                        }
                        else
                        {
                            action.MoveTo(swipePoints[i].X - swipePoints[i - 1].X, swipePoints[i].Y - swipePoints[i - 1].Y);
                        }
                    }
                    action.Perform();
                }
                else
                {
                    throw new Exception("Swipe operator miss some parameters");
                }
            }
            catch
            {
                if (arguments.Contains("-i"))
                    return;
                throw;
            }
                
        }

        public void LongPress(string Id, int duration, params string[] arguments)
        {
            try
            {
                IWebElement element = GetElement(Id);
                _driver.Tap(1, element.Location.X, element.Location.Y,duration*1000);
            }
            catch
            {
                if (arguments.Contains("-i"))
                    return;
                throw;
            }
        }

        public void Switch(string regionId, params string[] arguments)
        {
            throw new NotImplementedException();   
        }
        public void Restart()
        {
            _driver.CloseApp();
            _driver.Dispose();
            _driver = null;
            Init("RestartApp");            
        }

        public void Execute(string commandline)
        {
            Executor.Execute(this, commandline);
        }

        public void DragAndDrop(string StartId, params string[] arguments)
        {
            try
            {
                IWebElement startElement = GetElement(StartId);
                if (arguments != null && 1 == arguments.Length)
                {
                    IWebElement endElement = GetElement(arguments[0]);

                    int xEnd = endElement.Location.X;
                    int yEnd = endElement.Location.Y;

                    int xStart = startElement.Location.X;
                    int yStart = startElement.Location.Y;

                    TouchAction action = new TouchAction(_driver);
                    action.Press(xStart, yStart).MoveTo(xEnd - xStart, yEnd - yStart).Perform();
                }
                else
                {
                    double xOffset = double.Parse(arguments[0]);
                    double yOffset = double.Parse(arguments[1]);
                    TouchAction action = new TouchAction(_driver);
                    action.Press(startElement).MoveTo(startElement, xOffset, yOffset).Perform();
                }
            }
            catch
            {
                if (arguments.Contains("-i"))
                    return;
                throw;
            }
        }

        public void GetRegionElements()
        {
            throw new NotImplementedException();
        }

        public IntPtr[] GetHandles()
        {
            var processes = from p in Process.GetProcesses()
                where p.MainWindowTitle.Contains("player")
                select p.MainWindowHandle;
            return processes.ToArray();
        }

        public void Scroll(params string[] arguments)
        {
            throw new NotImplementedException();
        }
    }

    //public class TextWebElement:IWebElement
    //{
    //    private string _tagName;
    //    private string _text;
    //    private bool _enabled;
    //    private bool _selected;
    //    private Point _location;
    //    private Size _size;
    //    private bool _displayed;

    //    public TextWebElement(string text)
    //    {
    //        _text = text;
    //        _tagName = "Text";
    //        _enabled = false;
    //        _selected = false;
    //        _location = new Point();
    //        _size = new Size();
    //        _displayed = false;
    //    }

    //    public IWebElement FindElement(By @by)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public ReadOnlyCollection<IWebElement> FindElements(By @by)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Clear()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void SendKeys(string text)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Submit()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Click()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public string GetAttribute(string attributeName)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public string GetCssValue(string propertyName)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public string TagName
    //    {
    //        get { return _tagName; }
    //    }

    //    public string Text
    //    {
    //        get { return _text; }
    //    }



    //    public bool Enabled
    //    {
    //        get { return _enabled; }
    //    }

    //    public bool Selected
    //    {
    //        get { return _selected; }
    //    }

    //    public Point Location
    //    {
    //        get { return _location; }
    //    }

    //    public Size Size
    //    {
    //        get { return _size; }
    //    }

    //    public bool Displayed
    //    {
    //        get { return _displayed; }
    //    }
    //}
}
