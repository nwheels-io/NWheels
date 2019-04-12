
using System;

namespace NWheels.UI.Model
{
    public class TextContent : UIComponent<TextContentProps, Empty.State>
    {
        public TextContent(string text)
            : this(props => props.WithText(text))
        {
        }

        public TextContent(Action<TextContentProps> setProps)
        {
        }

        public static implicit operator TextContent(string text)
        {
            return new TextContent(text);
        }
    }

    public class TextContent<T> : TextContent
    {
        public TextContent(T data, string format)
            : base(format)
        {
        }

        public TextContent(T data, Func<T, string> format)
            : base("")
        {
        }
    }

    public class TextContentProps : PropsOf<TextContent>
    {
        public string Text;

        public TextContentProps WithText(string text) => default;
    }
}
