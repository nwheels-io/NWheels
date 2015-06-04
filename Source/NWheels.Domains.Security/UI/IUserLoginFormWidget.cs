using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Globalization;
using NWheels.UI;

namespace NWheels.Domains.Security.UI
{
    public interface IUserLoginFormWidget : IWidget, IBound<IUserLoginFormWidget, IUserLoginFormModel, IUserLoginFormState>
    {
        bool SignUpEnabled { get; set; }
        bool ForgotPasswordEnabled { get; set; }

        string LoginNameLabel { get; set; }
        string PasswordLabel { get; set; }
        string LoginNamePlaceholder { get; set; }
        string PasswordPlaceholder { get; set; }
        string SignUpText { get; set; }
        string ForgotPasswordText { get; set; }

        ICommand LogIn { get; set; }
        ICommand SignUp { get; set; }
        ICommand ForgotPassword { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [ViewModelContract]
    public interface IUserLoginFormModel
    {
        [PropertyContract.Required]
        string LoginName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [PropertyContract.Required, PropertyContract.Semantic.Password]
        string Password { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [ViewModelContract]
    public interface IUserLoginFormState
    {
        bool RememberMe { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class UserLoginFormFluentApi
    {
        public static IUserLoginFormWidget LoginNameLabel(this IUserLoginFormWidget widget, string value)
        {
            widget.LoginNameLabel = value;
            return widget;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IUserLoginFormWidget PasswordLabel(this IUserLoginFormWidget widget, string value)
        {
            widget.PasswordLabel = value;
            return widget;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static IUserLoginFormWidget LoginNamePlaceholder(this IUserLoginFormWidget widget, string value)
        {
            widget.LoginNamePlaceholder = value;
            return widget;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static IUserLoginFormWidget PasswordPlaceholder(this IUserLoginFormWidget widget, string value)
        {
            widget.PasswordPlaceholder = value;
            return widget;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static IUserLoginFormWidget SignUpText(this IUserLoginFormWidget widget, string value)
        {
            widget.SignUpText = value;
            return widget;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IUserLoginFormWidget ForgotPasswordText(this IUserLoginFormWidget widget, string value)
        {
            widget.ForgotPasswordText = value;
            return widget;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class UserLoginFormWidget : ICodeBehind<IUserLoginFormWidget>
    {
        private readonly ISecurityDomainTranslations _translations;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserLoginFormWidget(ISecurityDomainTranslations translations)
        {
            _translations = translations;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void OnDescribeUI(IUserLoginFormWidget widget)
        {
            var t = _translations;
            widget.LoginNameLabel(t.LoginName).PasswordLabel(t.Password).LoginNamePlaceholder(t.LoginName).PasswordPlaceholder(t.Password);
        }
    }
}

#if false

namespace NWheels.Domains.Security.UI
{

    
    public interface ILoginModel
    {
	    string LoginName { get; set; }
	    string Password { get; set; }
    }
    public interface ILoginState
    {
        bool RememberMe { get; set; }
    }
    public interface ILoginScreenTemplate : ITemplateUIWidget, Abstractions.IBound<ILoginScreenTemplate, ILoginModel, ILoginState>
    {
        string LoginIcon { get; set; }
        string LoginTitle { get; set; }
        string LoginSubTitle { get; set; }
        string LoginNamePlaceholder { get; set; }
        string PasswordPlaceholder { get; set; }
        string SignUpUrl { get; set; }
    }
    public interface ILoginUIScreen : IUIScreen, Abstractions.IBound<ILoginUIScreen, ILoginScreenModel, ILoginScreenState>
    {
        ILoginScreenTemplate Template { get; } 
    }
    public interface IMyApp1 : IUIApplication
    {
        ILoginUIScreen LoginScreen { get; }
    }


    
    public class MyApp1Builder : Abstractions.IApplicationBuilder<IMyApp1>
    {
        private readonly IMyApp1Translation _translation;

        public MyApp1Builder(IMyApp1Translation translation)
        {
            _translation = translation;
        }

        public void BuildApplication(IMyApp1 app)
        {
            var t = _translation;

            app.Icon(t.MyAppIcon).Title(t.MyApp).SubTitle(t.MyAppDescr).Copyright(t.MyAppDescr);
            //app.LoginScreen
        }
    }


    public interface IMyApp1Translation : ILocalizableUiResources
    {
        string MyApp { get; }
        string MyAppDescr { get; }
        string MyAppIcon { get; }
    }


    namespace Abstractions
    {
        public interface ICompositeOf<TChildren> : IEnumerable<TChildren>
        {
        }
        public interface IWidget
        {
            string IdName { get; }
        }
        public interface ICompositeWidget<TChildren> : IWidget, ICompositeOf<TChildren>
            where TChildren : IWidget
        {
        }
        public interface IBound<TWidget, TModel, TState>
        {
            IBindTo<TModel, TState, TValue> Bind<TValue>(Expression<Func<TWidget, TValue>> widgetProperty);
        }
        public interface IBindTo<TModel, TState, TValue>
        {
            void ToModel(Expression<Func<TModel, TValue>> modelProperty);
            void ToState(Expression<Func<TState, TValue>> stateProperty);
        }


        
        public abstract class Widget : IWidget
        {
            public string IdName { get; set; }
        }
        public abstract class CommandWidget : Widget
        {
            public Command Command { get; set; }
        }
        public abstract class CompositeWidget : Widget, IEnumerable<Widget>
        {
            public abstract IEnumerator<Widget> GetEnumerator();
            public abstract int WidgetCount { get; }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
        public abstract class ContainerWidget : CompositeWidget
        {
            private readonly List<Widget> _widgets = new List<Widget>();

            public override IEnumerator<Widget> GetEnumerator()
            {
                return _widgets.GetEnumerator();
            }
            public void Add(Widget widget)
            {
                _widgets.Add(widget);
            }
            public override int WidgetCount
            {
                get { return _widgets.Count; }
            }
            public Widget this[int index]
            {
                get { return _widgets[index]; }
                set { _widgets[index] = value; }
            }
        }
        public abstract class TemplateWidget<TPlaceholder> : CompositeWidget
        {
            private readonly Dictionary<TPlaceholder, Widget> _widgets = new Dictionary<TPlaceholder, Widget>();

            public override IEnumerator<Widget> GetEnumerator()
            {
                return _widgets.Values.GetEnumerator();
            }
            public override int WidgetCount
            {
                get { return _widgets.Count; }
            }
            public Widget this[TPlaceholder placeholder]
            {
                get { return _widgets[placeholder]; }
                set { _widgets[placeholder] = value; }
            }
        }
        public abstract class Command
        {
        }
    }

    namespace Toolbox
    {

        public class Link : Abstractions.CommandWidget
        {
            public string Url { get; set; }
            public string Text { get; set; }
        }
    }

}

#endif