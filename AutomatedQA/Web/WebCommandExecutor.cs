using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedQA.Web
{
    public class WebCommandExecutor :ICommandExecutor
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
            //Need to fix the bug
            if (!commandstr.ToLower().Equals("setvariable"))
            {
                for (int j = 0; j < parameters.Count; j++)
                {
                    string[] seperates = new string[2] { "{{", "}}" };
                    Dictionary<int, int> positions = new Dictionary<int, int>();
                    int lastPos = parameters[j].Length - 1;
                    int startPos = 0;
                    while (startPos <= lastPos)
                    {
                        int currentStartPos = parameters[j].IndexOf(seperates[0], startPos);
                        if (currentStartPos > -1 && currentStartPos < lastPos)
                        {
                            int currentEndPos = parameters[j].IndexOf(seperates[1], currentStartPos);
                            if (currentEndPos > -1 && currentEndPos <= lastPos)
                            {
                                positions.Add(currentStartPos, currentEndPos);
                                startPos = currentEndPos;
                            }
                            else
                            {
                                startPos = lastPos + 1;
                            }
                        }
                        else
                        {
                            startPos = lastPos + 1;
                        }
                    }




                    IList<string> variables = new List<string>();
                    foreach (var i in positions)
                    {
                        variables.Add(parameters[j].Substring(i.Key, i.Value - i.Key + 2));

                    }
                    foreach(string v in variables)
                    {
                        parameters[j] = parameters[j].Replace(v, command.GetVariable(v));
                    }
                }
            }

            switch (commandstr.Trim().ToLower())
            {
                case "goto":
                    command.Input("_url", parameters[1], parameters.Skip(2).ToArray());
                    break;
                case "click":
                    command.Click(parameters[1], parameters.Skip(2).ToArray());
                    break;
                case "doubleclick":
                    List<string> updatedParameters = parameters.Skip(2).ToList();
                    updatedParameters.Add("doubleclick");
                    command.Click(parameters[1], updatedParameters.ToArray());
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
                case "switch":
                    command.Switch(parameters[1]);
                    break;
                case "switchpage":
                    int pageNumber = -1;
                    if (!int.TryParse(parameters[1], out pageNumber) && pageNumber > 0)
                    {
                        throw new Exception("The page number is not correct when switchpage");
                    }
                    command.SwitchPage(pageNumber,parameters.Skip(2).ToArray());
                    break;
                case "select":                   
                    string id = parameters[1];
                    if (id.Contains('[') && id.Contains(']'))
                    {
                        string[] idParts = parameters[1].Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                        id = idParts[0];
                        try
                        {
                            int index = int.Parse(idParts[1]);
                            command.Select(id, index, parameters.Skip(2).ToArray());
                        }
                        catch
                        {
                            command.Select(id, idParts[1], parameters.Skip(3).ToArray());
                        }
                    }
                    else
                    {
                        command.Select(parameters[1], parameters[2], parameters.Skip(3).ToArray());
                    }
                    break;
                case "draganddrop":
                    command.DragAndDrop(parameters[1], parameters.Skip(2).ToArray());
                    break;

                case "setvariable":
                    command.Input(parameters[1], parameters[2],"setvariable");
                    break;

                case "executejavascript":
                    command.Input(parameters[1], null, "executejavascript");
                    break;

                case "refresh":
                    command.Input(null, null, "refresh");
                    break;

                case "acceptalert":
                    command.Click(null, "acceptalert");
                    break;

                case "dismissalert":
                    command.Click(null, "dismissalert");
                    break;

                case "keyboard":
                    parameters.Add("keyboard");
                    //deal with old wrong test case
                    if (parameters.Count > 3)
                    {
                        command.Input(parameters[1], parameters[2], parameters.Skip(3).ToArray());
                    }
                    //new right test case
                    else
                    {
                        command.Input(null, parameters[1], parameters.Skip(2).ToArray());
                    }
                    
                    break;

                default:
                    throw new Exception(string.Format("Unknown command"));
            }
        }
    }
}
