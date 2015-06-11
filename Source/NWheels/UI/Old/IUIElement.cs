using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface IUIElement
    {
        string IdName { get; set; }
        bool Enabled { get; set; }
        bool HiddenWhenDisabled { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class UIElementFluentApi
    {
        public static T IdName<T>(this T element, string value) where T : IUIElement
        {
            element.IdName = value;
            return element;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public static T Enabled<T>(this T element, bool value) where T : IUIElement
        {
            element.Enabled = value;
            return element;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public static T HiddenWhenDisabled<T>(this T element, bool value = true) where T : IUIElement
        {
            element.HiddenWhenDisabled = value;
            return element;
        }
    }
}
