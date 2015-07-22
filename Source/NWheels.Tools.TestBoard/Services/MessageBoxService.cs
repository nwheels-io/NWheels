using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NWheels.Extensions;
using NWheels.Tools.TestBoard.Modules.Main;

namespace NWheels.Tools.TestBoard.Services
{
    [Export(typeof(IMessageBoxService))]
    public class MessageBoxService : IMessageBoxService
    {
        public void InfoOK(string format, params object[] args)
        {
            MessageBox.Show(
                format.FormatIf(args), 
                MainModule.MainWindowTitle, 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool QuestionOKCancel(string format, params object[] args)
        {
            return (MessageBox.Show(
                format.FormatIf(args), 
                MainModule.MainWindowTitle, 
                MessageBoxButton.OKCancel, 
                MessageBoxImage.Question) == MessageBoxResult.OK);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool QuestionYesNo(string format, params object[] args)
        {
            return (MessageBox.Show(
                format.FormatIf(args),
                MainModule.MainWindowTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public System.Windows.MessageBoxResult QuestionYesNoCancel(string format, params object[] args)
        {
            return MessageBox.Show(
                format.FormatIf(args),
                MainModule.MainWindowTitle,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WarningOK(string format, params object[] args)
        {
            MessageBox.Show(
                format.FormatIf(args),
                MainModule.MainWindowTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool WarningOKCancel(string format, params object[] args)
        {
            return (MessageBox.Show(
                format.FormatIf(args),
                MainModule.MainWindowTitle,
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning) == MessageBoxResult.OK);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool WarningYesNo(string format, params object[] args)
        {
            return (MessageBox.Show(
                format.FormatIf(args),
                MainModule.MainWindowTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public System.Windows.MessageBoxResult WarningYesNoCancel(string format, params object[] args)
        {
            return MessageBox.Show(
                format.FormatIf(args),
                MainModule.MainWindowTitle,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ErrorOK(string format, params object[] args)
        {
            MessageBox.Show(
                format.FormatIf(args),
                MainModule.MainWindowTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool ErrorOKCancel(string format, params object[] args)
        {
            return (MessageBox.Show(
                format.FormatIf(args),
                MainModule.MainWindowTitle,
                MessageBoxButton.OKCancel,
                MessageBoxImage.Error) == MessageBoxResult.OK);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool ErrorYesNo(string format, params object[] args)
        {
            return (MessageBox.Show(
                format.FormatIf(args),
                MainModule.MainWindowTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Error) == MessageBoxResult.Yes);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public System.Windows.MessageBoxResult ErrorYesNoCancel(string format, params object[] args)
        {
            return MessageBox.Show(
                format.FormatIf(args),
                MainModule.MainWindowTitle,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Error);
        }
    }
}
