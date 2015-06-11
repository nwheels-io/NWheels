using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public static class Toolbox
    {
        public interface ITextWidget : IValueWidget
        {
            string Text { get; set; }
        }
        public interface ITextFieldWidget : IValueWidget
        {
        }
        public interface IButtonWidget : ICommandWidget
        {
        }
        public interface ILinkWidget : ICommandWidget
        {
        }
        public interface IFormWidget : ICommandWidget
        {
        }
    }
}
