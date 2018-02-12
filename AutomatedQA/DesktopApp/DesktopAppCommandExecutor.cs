using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomatedQA.DesktopApp
{
    public class DesktopAppCommandExecutor: ICommandExecutor
    {
        private bool _conditional;

        public OpenQA.Selenium.IWebDriver Driver
        {
            get { throw new NotImplementedException(); } 
            set {throw new NotImplementedException();}
        }

        public void Execute(ICommand command, string commandline)
        {
            if (commandline.StartsWith("--") || string.IsNullOrEmpty(commandline))
                return;
            List<string> parameters = CommonHelper.SplitCommand(commandline, " ");
            string commandstr = parameters[0].ToLower();
            // if the parameters have "true" and conditional is false---return
            if(parameters.Remove("true") && !_conditional)
            {
                return;
            }
            //if the parameters has "false" and the _conditional is true --- return
            if(parameters.Remove("false") && _conditional)
            {
                return;
            }
            switch (commandstr)
            {
                case "click":
                    command.Click(parameters[1], parameters.Skip(2).ToArray());
                    break;
                case "clickifenabled":
                case "clickifexists":
                    command.ClickIfExists(parameters[1], parameters.Skip(2).ToArray());
                    break;
                case "input":
                    command.Input(parameters[1], parameters[2], parameters.Skip(3).ToArray());
                    break;
                case "check":
                    command.Assert(parameters[1], parameters[2], parameters[3], parameters.Skip(4).ToArray());
                    break;
                case "exist":
                    command.Exist(parameters[1], parameters.Skip(2).ToArray());
                    break;
                case "notexist":
                    try
                    {
                        command.Exist(parameters[1], parameters.Skip(2).ToArray());
                    }
                    catch
                    {
                        break;
                    }
                    throw new Exception(string.Format("element {0} is existed.",parameters[1]));
                case "select":
                    int index;
                    if (parameters.Count > 2 && int.TryParse(parameters[2], out index))
                        command.Select(parameters[1], index, parameters.Skip(3).ToArray());
                    else
                        command.Select(parameters[1], 0, parameters.Skip(2).ToArray());
                    break;
                case "wait":
                    int timeout;
                    if (parameters.Count > 2 && int.TryParse(parameters[2], out timeout))
                        command.Wait(parameters[1], timeout, parameters.Skip(3).ToArray());
                    else
                        command.Wait(parameters[1], 60, parameters.Skip(2).ToArray());
                    break;
                case "sleep":
                    command.Sleep(int.Parse(parameters[1]));
                    break;
                case "switch":
                    command.Switch(parameters[1]);
                    break;
                case "showelements":
                    command.GetRegionElements();
                    break;
                case "restart":
                    command.Restart();
                    break;
                case "ifexists":
                    _conditional = command.IfExists(parameters[1], parameters.Skip(2).ToArray());
                    break;
                default:
                    throw new Exception(string.Format("Unknown command"));
            }
        }
    }
}
