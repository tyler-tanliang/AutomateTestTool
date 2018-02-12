using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Edge;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace AutomatedQA.Web
{
    public class WebCommand : ICommand
    {
        public ICommandExecutor Executor { get; set; }

        public string[] SpecialParameters
        {
            get { return _specialParameters; }
            set { _specialParameters = value; }
        }

        private IWebDriver _driver;
        private IJavaScriptExecutor _javaScript;
        private readonly AppElementCollection _appElements;
        private readonly string _name;
        private readonly string _driverPath;
        private List<string> _currentRegion;
        private string[] _specialParameters;
        private Dictionary<string, string> _variables = null;

        public WebCommand(string name, string driverPath, AppElementCollection app)
        {
            _name = name;
            _driverPath = driverPath;
            _appElements = app;
            _specialParameters = new[] { "-i", "-r" };
            Executor = new WebCommandExecutor();
            Init();
        }

        private void Init()
        {
            switch (_name.ToUpper())
            {
                case "CHROME":
                    ChromeDriverService servicce = ChromeDriverService.CreateDefaultService();
                    _driver = new ChromeDriver(servicce);
                    break;

                case "EDGE":
                    EdgeOptions edgeOptions = new EdgeOptions();
                    EdgeDriverService serv = EdgeDriverService.CreateDefaultService(@"D:\Program Files (x86)\Microsoft Web Driver", "MicrosoftWebDriver.exe");
                    _driver = new EdgeDriver(serv, edgeOptions);

                    break;
                case "IE":
                    InternetExplorerOptions ieOption = new InternetExplorerOptions();
                    ieOption.IgnoreZoomLevel = true;
                    //ieOption.InitialBrowserUrl = "about:blank";
                    //ieOption.IntroduceInstabilityByIgnoringProtectedModeSettings = true;
                    _driver = new InternetExplorerDriver(ieOption);
                    break;
                case "FIREFOX":

                    //string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    FirefoxProfileManager m = new FirefoxProfileManager();
                    FirefoxOptions options = new FirefoxOptions();
                    options.Profile = new FirefoxProfile();
                    //options.Profile = m.GetProfile(m.ExistingProfiles[0]);
                    _driver = new FirefoxDriver(options);
                    break;
                default:
                    throw new ArgumentException("Unknown driver: " + _name);
            }
            if (_driver != null)
            {
                _driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(3);
                _driver.Manage().Window.Maximize();
            }
            _javaScript = (IJavaScriptExecutor)_driver;
            _variables = new Dictionary<string, string>();
            Executor.Driver = _driver;
        }

        private IWebElement GetElement(string id)
        {
            IWebElement element = null;

            //if the parameter is number in id[], it's true, if the parameter is string in id[], it's false.
            bool subscriptIndexNumber = true;
            int[] index = { 0 };
            string[] idParameters = id.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            if (id.Contains('[') && id.Contains(']'))
            {
                id = idParameters[0];
                try
                {
                    index = Array.ConvertAll(idParameters.Skip(1).ToArray(), int.Parse);
                    for (int i = 0; i < index.Length; i++)
                    {
                        index[i]--;
                    }
                    subscriptIndexNumber = true;
                }
                catch
                {
                    subscriptIndexNumber = false;
                    //if the test case start with " and end with ", set it's a string, so need remove the ""
                    if (idParameters[1].StartsWith("\"") && idParameters[1].EndsWith("\""))
                    {
                        idParameters[1] = idParameters[1].Substring(1, idParameters[1].Length - 2);
                    }
                }
            }
            string signature = GetSignature(id);
            if (string.IsNullOrEmpty(signature))
            {
                throw new Exception(string.Format("Please check config file, there isn't an element named '{0}'.", id));
            }
            string[] byParams = new string[2];
            int pos = signature.IndexOf(':');
            byParams[0] = signature.Substring(0, pos);
            byParams[1] = signature.Substring(pos + 1);
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
                    throw new Exception(string.Format("Cannot find element {0}", id));
                }

                //if target is a collections, to get the one matched index
                if (elementObj is System.Collections.IList)
                {

                    //if the parameter in id[] is number
                    if (subscriptIndexNumber)
                    {
                        elementObj = index.Aggregate(elementObj, (current, i) => ((System.Collections.IList)current)[i]);
                    }
                    //if the parameter in id[] is string
                    else
                    {
                        IList<IWebElement> elementsList = elementObj as IList<IWebElement>;
                        string parameterInId = idParameters[1];
                        bool foundElement = false;
                        for (int j = 0; j < elementsList.Count; j++)
                        {
                            try
                            {
                                foundElement = elementsList[j].Text.Equals(parameterInId);
                                if (!foundElement)
                                {
                                    foundElement = elementsList[j].GetAttribute("value").Equals(parameterInId);
                                }
                            }
                            catch
                            {                                
                            }
                            if (foundElement)
                            {
                                elementObj = elementsList[j];
                                break;
                            }
                        }
                    }
                }



                if (elementObj is IWebElement)
                    element = elementObj as IWebElement;
                //else
                //    element = new TextWebElement(elementObj.ToString());
            }
            else
            {
                By by =
                    typeof(By).InvokeMember(byParams[0],
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, null,
                        new object[] { byParams[1] }) as By;
                //if the parameter in id[] is number
                if (subscriptIndexNumber)
                {
                    element = index[0] == 0 ? _driver.FindElement(by) : _driver.FindElements(by)[index[0]];
                }
                //if the parameter in id[] is string
                else
                {
                    IList<IWebElement> elementsList = _driver.FindElements(by);
                    string parameterInId = idParameters[1];
                    bool foundElement = false;
                    for (int j = 0; j < elementsList.Count; j++)
                    {
                        try
                        {
                            foundElement = elementsList[j].Text.Equals(parameterInId);
                            if (!foundElement)
                            {
                                foundElement = elementsList[j].GetAttribute("value").Equals(parameterInId);
                            }
                        }
                        catch
                        {                            
                        }
                        if (foundElement)
                        {
                            element = elementsList[j];
                            break;
                        }
                    }
                    if (element == null)
                    {
                        throw new Exception("Cannot find the element: " + id);
                    }
                }
            }

            try
            {
                //try to scroll into view if element is not displayed on the screen
                if (!element.Displayed)
                {
                    _javaScript.ExecuteScript("arguments[0].scrollIntoView(true);", element);
                }

            }
            catch
            { }
            //IJavaScriptExecutor jj = (IJavaScriptExecutor)element;
            //jj.ExecuteScript("");
            return element;
        }

        private string GetSignature(string id)
        {
            try
            {
                var elements = _appElements.Regions.Where(r => _currentRegion.Contains(r.Id) || r.Id == "Master").SelectMany(r => r.Elements);

                Element elementInfo = elements.FirstOrDefault(e => e.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
                string signature = string.Empty;
                if (null != elementInfo)
                {
                    signature = elementInfo.Signature;
                }
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
                object gotValue = _javaScript.ExecuteScript(javascriptParameter.Substring(3));
                valueOfJSGet = gotValue.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Java Script is error when get value with js, details is: " + ex.Message);
            }
            return valueOfJSGet;
        }


        public void Dispose()
        {
            _driver.Close();
            _driver.Quit();
            _driver.Dispose();
        }

        private void AcceptAlert()
        {
            IAlert alert = _driver.SwitchTo().Alert();
            alert.Accept();
        }
        private void DismissAlert()
        {
            IAlert alert = _driver.SwitchTo().Alert();
            alert.Dismiss();
        }

        public void Input(string id, string value, params string[] arguments)
        {
            try
            {
                if (arguments.Length > 0 && arguments[0].Equals("setvariable"))
                {
                    if (!id.StartsWith("{{") || !id.EndsWith("}}"))
                    {
                        throw new Exception("The variable should be packaged by {{}}");
                    }
                    SetVariable(id, value);
                    return;
                }
                else if (value == null && arguments.Length > 0 && arguments[0].Equals("executejavascript"))
                {
                    _javaScript.ExecuteScript(id);
                }
                else if (id == null && value == null && arguments.Length > 0 && arguments[0].Equals("refresh"))
                {
                    _driver.Navigate().Refresh();
                }
                else
                {
                    if (id != null && id.Equals("_url", StringComparison.CurrentCultureIgnoreCase))
                    {
                        _driver.Navigate().GoToUrl(value);
                        if (value.Contains("9001"))
                        {
                            Thread.Sleep(1000);
                            IList<Cookie> test = _driver.Manage().Cookies.AllCookies;
                            //Cookie[] old = new Cookie[test.Count];
                            //test.CopyTo(old,0);
                            _driver.Manage().Cookies.DeleteAllCookies();
                            Cookie[] userCookie = new Cookie[7];
                            userCookie[0]= new Cookie("accessToken", "RNhEn-fkva_cwKyO6AqkRtNaV8e8EuhU6th7Wuy2_Rbsqn1nkcq9GOtKEwRj6YNTFbYjnFIThDYWJe_KxLqjIguhtIryplA4tDDMjPTZU10oUe0LzU5psYJJVxxvVWAu0_arsFPLTFeI_0HSFC2XD9wcL14iMkvnrPoaFR_Lm39UzqmLemmSdXVLwkjdiafYcgP8NdB8KlHyX8-qQxuPbCvor0Q9E-G50NLcj9JTNRflhAKejBLMh7EWE12oeJsJ", "192.168.1.104","/",null);
                            userCookie[1] = new Cookie("UserRolePriority", "6", "192.168.1.104", "/", null);
                            userCookie[2] = new Cookie("roleId", "6", "192.168.1.104", "/", null);
                            userCookie[3] = new Cookie("roleLevel", "6", "192.168.1.104", "/", null);
                            userCookie[4] = new Cookie("userId", "22", "192.168.1.104", "/", null);
                            userCookie[5] = new Cookie("userName", "Admin", "192.168.1.104", "/", null);
                            userCookie[6] = new Cookie("userRole", "Management", "192.168.1.104", "/", null);
                            foreach (Cookie c in userCookie)
                            {
                                _driver.Manage().Cookies.AddCookie(c);
                            }
                            _driver.Navigate().GoToUrl(value);
                            //foreach (Cookie c in old)
                            //{
                            //    _driver.Manage().Cookies.AddCookie(c);
                            //}
                            //192.168.1.104
                            //    /
                            //Cookie cookie = new Cookie("accessToken", "qP1l76XBNjkvF2QYv7KMPMAz9PkMl-nSsRfDxMQlJD5WPuhjuzjmlJMzmfmS32CUluxvNqjPrqnQ7aiN2u8ZFAacbfEAvMjJEBotu0I6LdC064ab8Do6HLCuAkIyT6WvTX2GqL1Jy45jmwkJqWyepZIBv9_bf8bMIYBvphBe4IJ07fgKDto2WNZgwBhbsCMcXDuEdIfRoBTwL-UhrOTcFRxnrYXhkAiae2I_tSWmfb44x9EgCrsgQHjHp_TzTwzu");
                            //Cookie cookie = new Cookie("accessToken", "qP1l76XBNjkvF2QYv7KMPMAz9PkMl-nSsRfDxMQlJD5WPuhjuzjmlJMzmfmS32CUluxvNqjPrqnQ7aiN2u8ZFAacbfEAvMjJEBotu0I6LdC064ab8Do6HLCuAkIyT6WvTX2GqL1Jy45jmwkJqWyepZIBv9_bf8bMIYBvphBe4IJ07fgKDto2WNZgwBhbsCMcXDuEdIfRoBTwL-UhrOTcFRxnrYXhkAiae2I_tSWmfb44x9EgCrsgQHjHp_TzTwzu");
                            //Cookie c1 = new Cookie("UserRolePriority", "6");
                            //Cookie c2 = new Cookie("roleId", "6");
                            //Cookie c3 = new Cookie("roleLevel", "6");
                            //Cookie c4 = new Cookie("userId", "22");
                            //Cookie c5 = new Cookie("userName", "Admin");
                            //Cookie c6 = new Cookie("userRole", "Management");
                            //_driver.Manage().Cookies.AddCookie(cookie);
                            //_driver.Manage().Cookies.AddCookie(c1);
                            //_driver.Manage().Cookies.AddCookie(c2);
                            //_driver.Manage().Cookies.AddCookie(c3);
                            //_driver.Manage().Cookies.AddCookie(c4);
                            //_driver.Manage().Cookies.AddCookie(c5);
                            //_driver.Manage().Cookies.AddCookie(c6);
                            //_driver.Navigate().GoToUrl(value);
                        }
                        if (_name.ToUpper().Equals("IE"))
                        {
                            object sslError = _javaScript.ExecuteScript("return document.getElementById('overridelink')");
                            if (sslError != null)
                            {
                                _javaScript.ExecuteScript("document.getElementById('overridelink').click()");
                            }
                        }
                    }
                    else
                    {
                        #region KeyboardEvent
                        if (arguments.Length > 0 && arguments.Contains("keyboard"))
                        {
                            Actions actions = new Actions(_driver);
                            switch (value.ToLower())
                            {
                                case "add":
                                    actions.SendKeys(Keys.Add);
                                    break;
                                case "alt":
                                    actions.SendKeys(Keys.Alt);
                                    break;
                                case "arrowdown":
                                    actions.SendKeys(Keys.ArrowDown);
                                    break;
                                case "arrowleft":
                                    actions.SendKeys(Keys.ArrowLeft);
                                    break;
                                case "arrowright":
                                    actions.SendKeys(Keys.ArrowRight);
                                    break;
                                case "arrowup":
                                    actions.SendKeys(Keys.ArrowUp);
                                    break;
                                case "backspace":
                                    actions.SendKeys(Keys.Backspace);
                                    break;
                                case "cancel":
                                    actions.SendKeys(Keys.Cancel);
                                    break;
                                case "clear":
                                    actions.SendKeys(Keys.Clear);
                                    break;
                                case "command":
                                    actions.SendKeys(Keys.Command);
                                    break;
                                case "control":
                                    actions.SendKeys(Keys.Control);
                                    break;
                                case "decimal":
                                    actions.SendKeys(Keys.Decimal);
                                    break;
                                case "delete":
                                    actions.SendKeys(Keys.Delete);
                                    break;
                                case "divide":
                                    actions.SendKeys(Keys.Divide);
                                    break;
                                case "down":
                                    actions.SendKeys(Keys.Down);
                                    break;
                                case "end":
                                    actions.SendKeys(Keys.End);
                                    break;
                                case "enter":
                                    actions.SendKeys(Keys.Enter);
                                    break;
                                case "equal":
                                    actions.SendKeys(Keys.Equal);
                                    break;
                                case "escape":
                                    actions.SendKeys(Keys.Escape);
                                    break;
                                case "f1":
                                    actions.SendKeys(Keys.F1);
                                    break;
                                case "f10":
                                    actions.SendKeys(Keys.F10);
                                    break;
                                case "f11":
                                    actions.SendKeys(Keys.F11);
                                    break;
                                case "f12":
                                    actions.SendKeys(Keys.F12);
                                    break;
                                case "f2":
                                    actions.SendKeys(Keys.F2);
                                    break;
                                case "f3":
                                    actions.SendKeys(Keys.F3);
                                    break;
                                case "f4":
                                    actions.SendKeys(Keys.F4);
                                    break;
                                case "f5":
                                    actions.SendKeys(Keys.F5);
                                    break;
                                case "f6":
                                    actions.SendKeys(Keys.F6);
                                    break;
                                case "f7":
                                    actions.SendKeys(Keys.F7);
                                    break;
                                case "f8":
                                    actions.SendKeys(Keys.F8);
                                    break;
                                case "f9":
                                    actions.SendKeys(Keys.F9);
                                    break;
                                case "help":
                                    actions.SendKeys(Keys.Help);
                                    break;
                                case "home":
                                    actions.SendKeys(Keys.Home);
                                    break;
                                case "insert":
                                    actions.SendKeys(Keys.Insert);
                                    break;
                                case "left":
                                    actions.SendKeys(Keys.Left);
                                    break;
                                case "leftalt":
                                    actions.SendKeys(Keys.LeftAlt);
                                    break;
                                case "leftcontrol":
                                    actions.SendKeys(Keys.LeftControl);
                                    break;
                                case "leftshift":
                                    actions.SendKeys(Keys.LeftShift);
                                    break;
                                case "meta":
                                    actions.SendKeys(Keys.Meta);
                                    break;
                                case "multiply":
                                    actions.SendKeys(Keys.Multiply);
                                    break;
                                case "null":
                                    actions.SendKeys(Keys.Null);
                                    break;
                                case "numberpad0":
                                    actions.SendKeys(Keys.NumberPad0);
                                    break;
                                case "numberpad1":
                                    actions.SendKeys(Keys.NumberPad1);
                                    break;
                                case "numberpad2":
                                    actions.SendKeys(Keys.NumberPad2);
                                    break;
                                case "numberpad3":
                                    actions.SendKeys(Keys.NumberPad3);
                                    break;
                                case "numberpad4":
                                    actions.SendKeys(Keys.NumberPad4);
                                    break;
                                case "numberpad5":
                                    actions.SendKeys(Keys.NumberPad5);
                                    break;
                                case "numberpad6":
                                    actions.SendKeys(Keys.NumberPad6);
                                    break;
                                case "numberpad7":
                                    actions.SendKeys(Keys.NumberPad7);
                                    break;
                                case "numberpad8":
                                    actions.SendKeys(Keys.NumberPad8);
                                    break;
                                case "numberpad9":
                                    actions.SendKeys(Keys.NumberPad9);
                                    break;
                                case "pagedown":
                                    actions.SendKeys(Keys.PageDown);
                                    break;
                                case "pageup":
                                    actions.SendKeys(Keys.PageUp);
                                    break;
                                case "pause":
                                    actions.SendKeys(Keys.Pause);
                                    break;
                                case "return":
                                    actions.SendKeys(Keys.Return);
                                    break;
                                case "right":
                                    actions.SendKeys(Keys.Right);
                                    break;
                                case "semicolon":
                                    actions.SendKeys(Keys.Semicolon);
                                    break;
                                case "separator":
                                    actions.SendKeys(Keys.Separator);
                                    break;
                                case "shift":
                                    actions.SendKeys(Keys.Shift);
                                    break;
                                case "space":
                                    actions.SendKeys(Keys.Space);
                                    break;
                                case "subtract":
                                    actions.SendKeys(Keys.Subtract);
                                    break;
                                case "tab":
                                    actions.SendKeys(Keys.Tab);
                                    break;
                                case "up":
                                    actions.SendKeys(Keys.Up);
                                    break;
                                default:
                                    actions.SendKeys(value);
                                    break;
                            }
                            actions.Perform();
                            return;
                        }
                        #endregion
                        else
                        {
                            IWebElement element = GetElement(id);
                            element.Clear();
                            element.SendKeys(value);
                        }

                    }

                }
            }

            catch(Exception ex)
            {
                if (arguments.Contains("-i"))
                    return;
                throw ex; 
            }
        }

        public string GetVariable(string id)
        {
            string value = string.Empty;

            if (_variables.ContainsKey(id.ToLower()))
            {
                value = _variables[id.ToLower()];
            }
            return value;
        }
        

        private void SetVariable(string id, string value)
        {
            if (!string.IsNullOrEmpty(value) && value.IndexOf("js", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                value = GetValueWithJS(value);
            }

            if (_variables.ContainsKey(id.ToLower()))
            {
                _variables[id.ToLower()] = value;
            }
            else
            {
                _variables.Add(id.ToLower(), value);
            }
        }

        public void Click(string id, params string[] arguments)
        {           
            if (arguments.Length > 0 && arguments[0].Equals("acceptalert"))
            {
                try
                {
                    AcceptAlert();
                    return;
                }
                catch
                {
                    if (arguments.Contains("-i"))
                        return;
                    throw;
                }
            }
            if (arguments.Length > 0 && arguments[0].Equals("dismissalert"))
            {
                try
                {
                    DismissAlert();
                    return;
                }
                catch
                {
                    if (arguments.Contains("-i"))
                        return;
                    throw;
                }
            }
            try
            {
                IWebElement element = GetElement(id);
                bool dblClick = false;
                if (arguments.Length > 0)
                {
                    foreach(string s in arguments)
                    {
                        if (s.Equals("doubleclick"))
                        {
                            dblClick = true;
                            break;
                        }
                    }
                }
                if (dblClick)
                {
                    Actions action = new Actions(_driver);
                    action.DoubleClick(element).Perform();
                }
                else
                {
                    element.Click();
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

            try
            {
                string actualValue = null;
                IWebElement element = null;
                if (!string.IsNullOrEmpty(id))
                    element = GetElement(id);
                bool matched = false;
                switch (name.ToLower())
                {
                    case "url":
                        actualValue =_driver.Url.ToLower();
                        matched = actualValue.Contains(value.ToLower());
                        
                        break;
                    case "page":
                        Region region = _appElements[value];
                        if (region != null)
                        {
                            actualValue=_driver.Url.ToLower();
                            matched = actualValue.Contains(region.Name.ToLower());
                        }
                        else
                        {
                            throw new Exception(string.Format("Region {0} is not exist.", value));
                        }
                        if (matched)
                        {
                            _currentRegion = new List<string> { region.Id };
                            if (region.ReferRegionList != null)
                            {
                                _currentRegion.AddRange(region.ReferRegionList);
                            }
                        }
                        break;
                    case "text":
                        if (element.TagName.ToLower() == "input")
                        {
                            switch (element.GetAttribute("type").ToLower())
                            {
                                case "text":
                                    actualValue = element.GetAttribute("value");
                                    break;
                                case "radio":
                                case "checkbox":
                                    actualValue = element.Selected.ToString();
                                    break;
                                default:
                                    actualValue = element.GetAttribute("value");
                                    break;
                            }

                        }
                        else if (element.TagName.ToLower() == "select")
                        {
                            SelectElement selectElement = new SelectElement(element);
                            actualValue = selectElement.SelectedOption.Text;
                        }
                        else
                        {
                            actualValue = element.Text;
                        }
                        //Compare the value between expected and actual with regex or other regular.
                        matched = CommonHelper.IsMatchRegex(value, actualValue);
                        break;
                    default:
                        actualValue=element.GetAttribute(name);
                        matched = actualValue.Equals(value, StringComparison.CurrentCultureIgnoreCase);
                        break;
                }
                
                //sometimes when check a value is empty, on web page the element will be hidden, not clean the value/text
                if (element != null && !matched && string.IsNullOrEmpty(value) && !element.Displayed)
                    matched = true;
                if (!matched)
                {
                    throw new Exception("Check failed: actual value of " + id + " is " + actualValue);
                }
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

        public void Select(string id, string value, params string[] arguments)
        {
            try
            {
                IWebElement element = GetElement(id);
                if (element.TagName.ToLower() == "input" && (element.GetAttribute("type").ToLower() == "checkbox" || element.GetAttribute("type").ToLower() == "radio"))
                {
                    bool expect = value.ToLower() == "true";
                    if (expect != element.Selected)
                    {
                        if (!element.Displayed)
                        {
                            element = _javaScript.ExecuteScript("return $(arguments[0]).parent()[0];", element) as IWebElement;
                        }
                        element.Click();
                    }
                }
                else //dorp down
                {
                    SelectElement selectElement= new SelectElement(element);
                    try
                    {
                        selectElement.SelectByText(value);
                    }
                    catch
                    {
                        selectElement.SelectByValue(value);
                    }
                }
            }
            catch(Exception ex)
            {                
                throw ex;
            }
        }

        public void Select(string id,int index, params string[] arguments)
        {
            try
            {
                IWebElement element = GetElement(id);
                if (element.FindElements(By.TagName("option")).Count == 0)
                {
                    throw new OpenQA.Selenium.NoSuchElementException("There is not options for the element: " + id);
                }
                SelectElement selectElement =new SelectElement(element);
                selectElement.SelectByIndex(index-1);                
            }
            catch (Exception ex)
            {                
                throw ex;
            }
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

        public void Switch(string regionId, params string[] arguments)
        {
            Process process = Process.GetProcesses().FirstOrDefault(p => p.MainWindowTitle.Contains(_driver.Title));
            if (process != null)
            {
                WinApi.SetFocus(process.MainWindowHandle);
            }
            _driver.Manage().Window.Maximize();
        }

        public void SwitchPage(int pageNumber, params string[] arguments)
        {
            IList<string> windows = _driver.WindowHandles;
            if (windows.Count >= 1 && pageNumber > 0)
            {
                string activceWindow = windows[pageNumber - 1];
                _driver.SwitchTo().Window(activceWindow);
            }
        }

        public void Swipe(string StartId, params string[] arguments)
        {
            throw new NotImplementedException();            
        }

        public void DragAndDrop(string StartId, params string[] arguments)
        {
            try
            {
                IWebElement startElement = GetElement(StartId);
                Actions action = new Actions(_driver);
                if (arguments.Length==1)
                {
                    IWebElement endElement = GetElement(arguments[0]);                    
                    action.DragAndDrop(startElement, endElement).Perform();
                }
                else if(arguments.Length>1)
                {
                    int xOffset = int.Parse(arguments[0]);
                    int yOffset = int.Parse(arguments[1]);
                    action.DragAndDropToOffset(startElement, xOffset, yOffset).Perform();
                }
            }
            catch
            {
                if (arguments.Contains("-i"))
                    return;
                throw;
            }

        }

        public void LongPress(string Id, int duration,params string[] arguments)
        {
            throw new NotImplementedException();
        }

        public void Restart()
        {
            _driver.Quit();
            _driver.Dispose();
            Init();
        }

        public void Execute(string commandline)
        {
            Executor.Execute(this, commandline);
        }

        public void GetRegionElements()
        {
            throw new NotImplementedException();
        }

        public IntPtr[] GetHandles()
        {
            var processes = from p in Process.GetProcesses()
                where p.MainWindowTitle.Contains(_driver.Title)
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
