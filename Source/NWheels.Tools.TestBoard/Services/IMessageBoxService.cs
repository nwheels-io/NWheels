using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NWheels.Tools.TestBoard.Services
{
    public interface IMessageBoxService
    {
        void InfoOK(string format, params object[] args);
        bool QuestionOKCancel(string format, params object[] args);
        bool QuestionYesNo(string format, params object[] args);
        MessageBoxResult QuestionYesNoCancel(string format, params object[] args);
        void WarningOK(string format, params object[] args);
        bool WarningOKCancel(string format, params object[] args);
        bool WarningYesNo(string format, params object[] args);
        MessageBoxResult WarningYesNoCancel(string format, params object[] args);
        void ErrorOK(string format, params object[] args);
        bool ErrorOKCancel(string format, params object[] args);
        bool ErrorYesNo(string format, params object[] args);
        MessageBoxResult ErrorYesNoCancel(string format, params object[] args);
    }
}
