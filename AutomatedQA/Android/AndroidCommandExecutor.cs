using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedQA.Android
{
    public class AndroidCommandExecutor : ICommandExecutor
    {
        public OpenQA.Selenium.IWebDriver Driver
        {
            get;
            set;
        }
        public void Execute(ICommand command, string commandline)
        {
            if (commandline.StartsWith("--") || string.IsNullOrEmpty(commandline))
                return;
            List<string> parameters = CommonHelper.SplitCommand(commandline, " ");
            string commandstr = parameters[0];
            switch (commandstr.ToLower())
            {
                case "click":
                    command.Click(parameters[1], parameters.Skip(2).ToArray());
                    break;
                case "input":
                    command.Input(parameters[1], parameters[2], parameters.Skip(3).ToArray());
                    break;
                case "assert":
                    if (parameters[1].Equals("url", StringComparison.CurrentCultureIgnoreCase))
                        command.Assert(null, parameters[1], parameters[2], parameters.Skip(3).ToArray());
                    else if (parameters[1].Equals("page", StringComparison.CurrentCultureIgnoreCase))
                        command.Assert(null, parameters[1], parameters[2], parameters.Skip(3).ToArray());
                    else if (parameters.Count(p => !command.SpecialParameters.Contains(p)) == 3)
                        command.Assert(parameters[1], "text", parameters[2], parameters.Skip(3).ToArray());
                    else
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
                    throw new Exception(string.Format("element {0} is existed.", parameters[1]));
                case "wait":
                    int timeout;
                    if (int.TryParse(parameters[2], out timeout))
                        command.Wait(parameters[1], timeout, parameters.Skip(3).ToArray());
                    else
                        command.Wait(parameters[1], 60, parameters.Skip(2).ToArray());
                    break;
                case "sleep":
                    command.Sleep(int.Parse(parameters[1]));
                    break;
                case "switchpage":
                    //Need implement
                    //command.SwitchPage(parameters[1]);
                    break;
                case "swipe":
                    command.Swipe(parameters[1], parameters.Skip(2).ToArray());
                    break;

                case "draganddrop":
                    command.DragAndDrop(parameters[1], parameters.Skip(2).ToArray());
                    break;

                case "longpress":
                    command.LongPress(parameters[1], parameters[2].Equals(string.Empty) ? 5 : int.Parse(parameters[2]), parameters.Skip(3).ToArray());
                    break;
                case "select":                    
                    string id = parameters[1];
                    if (id.Contains('[') && id.Contains(']'))
                    {
                        string[] idParts = parameters[1].Split(new[] {'[', ']'}, StringSplitOptions.RemoveEmptyEntries);
                        //id = idParts[0];
                        string option = idParts[1];
                        command.Select(id, option, parameters.Skip(2).ToArray());
                    }
                    else
                    {
                        throw new NotImplementedException("Selection function cannot be used as this right now, will implement soon.");
                    }                    
                    break;
                default:
                    throw new Exception(string.Format("Unknown command"));
            }
        }
    }
}
