using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Remote;

namespace AutomatedQA
{
    public interface ICommandExecutor
    {
        void Execute(ICommand command, string commandline);
        OpenQA.Selenium.IWebDriver Driver { get; set; }
    }
}
