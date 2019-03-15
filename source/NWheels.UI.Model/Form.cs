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
        public FormProps<T> WithActions(params FormActionProps<T>[] actions) => default;
    }

    public class FormFieldProps<T>
    {
    }

    public class FormActionProps<T>
    {
        public string Id;
        public Func<T, bool> OnUpdate;
        public Func<T, Task> OnExecute;
    }

    public class Form<T> : UIComponent<FormProps<T>, Empty.State>
    {
        public Form(Action<FormProps<T>> setProps)
        {
        }
    }
}
