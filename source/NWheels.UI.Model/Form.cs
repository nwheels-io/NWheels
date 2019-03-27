using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NWheels.UI.Model
{
    public class FormProps<T> : PropsOf<Form<T>>
    {
        public T Data;
        
        public IList<FormFieldProps<T>> Fields;
        public IList<FormActionProps<T>> Actions;

        public FormProps<T> WithAutoFields() => default;
        public FormProps<T> WithFields(params Func<T, object>[] fields) => default;
        public FormProps<T> WithAction(string id, Action<FormActionProps<T>> setProps = null) => default;
        public FormProps<T> WithSubmitAction(string id = "submit", Action<FormActionProps<T>> setProps = null) => default;
        public FormProps<T> WithResetAction(string id = "reset", Action<FormActionProps<T>> setProps = null) => default;
    }

    public enum FormActionSemantics
    {
        Submit,
        Reset,
        Apply,
        Cancel,
        Close
    }
    
    public class FormFieldProps<T>
    {
    }

    public class FormActionProps<T>
    {
        public string Id;
        public FormActionSemantics? Semantics;
        public Func<T, bool> OnUpdate;
        public Func<T, Task> OnExecute;

        public FormActionProps<T> WithSemantics(FormActionSemantics semantics) => default;
        public FormActionProps<T> WithUpdate(Func<T, bool> onUpdate) => default;
        public FormActionProps<T> WithUpdate(Func<T, Task<bool>> onUpdate) => default;
        public FormActionProps<T> WithExecute(Func<T, Task> onExecute) => default;
    }

    public class Form<T> : UIComponent<FormProps<T>, Empty.State>
    {
        public Form(Action<FormProps<T>> setProps)
        {
        }
    }
}
