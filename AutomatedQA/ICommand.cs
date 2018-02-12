using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace AutomatedQA
{
    public interface ICommand: IDisposable
    {
        ICommandExecutor Executor { get; set; }
        string[] SpecialParameters { get; set; }
        void Input(string id, string value, params string[] arguments);
        void Click(string id, params string[] arguments);
        void ClickIfExists(string id, params string[] arguments);
        bool IfExists(string id, params string[] arguments);
        void Assert(string id, string name, string value, params string[] arguments);
        void Exist(string id, params string[] arguments);
        void Select(string id, int index, params string[] arguments);
        void Select(string id, string text, params string[] arguments);
        void Wait(string id, int timeOutSeconds = 60, params string[] arguments);
        void Sleep(int secondsValue);
        void Switch(string regionId, params string[] arguments);
        void SwitchPage(int pageNumber, params string[] arguments);
        void Swipe(string startId, params string[] arguments);
        void DragAndDrop(string startId, params string[] arguments);
        string GetVariable(string id);
        void Scroll(params string[] arguments);
        void LongPress(string id, int duration, params string[] arguments);
        void Restart();
        void Execute(string commandline);
        void GetRegionElements();
        IntPtr[] GetHandles();
               
    }
}
