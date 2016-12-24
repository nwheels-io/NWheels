using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using NWheels.Authorization.Core;
using NWheels.Core;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.UI;

namespace NWheels.Processing
{
    public interface ITransactionScript
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ITransactionScript<TContext, TInput, TOutput> : ITransactionScript
    {
        TInput InitializeInput(TContext context);
        void SaveInputDraft(TInput input);
        void DiscardInputDraft();
        TOutput Preview(TInput input);
        TOutput Execute(TInput input);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TransactionScriptAttribute : Attribute
    {
        public bool SupportsInitializeInput { get; set; }
        public bool SupportsPreview { get; set; }
        public bool SupportsInputDraft { get; set; }
        public string AuditName { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class TransactionScript<TContext, TInput, TOutput> : ITransactionScript<TContext, TInput, TOutput>
    {
        public virtual TInput InitializeInput(TContext context)
        {
            return default(TInput);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void SaveInputDraft(TInput input)
        {
            var uiContext = GetUIContext();
            var json = uiContext.EntityService.GetJsonFromObject(input.As<IDomainObject>(), uiContext.Query);

            Session.Current.SetUserStorageItem(this.GetType().FriendlyName(), json);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void DiscardInputDraft()
        {
            Session.Current.RemoveUserStorageItem(this.GetType().FriendlyName());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual TOutput Preview(TInput input)
        {
            return default(TOutput);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract TOutput Execute(TInput input);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual bool TryLoadInputDraft(out TInput draft)
        {
            string json;
            IDomainObject deserializedDraft = null;

            if (Session.Current.TryGetUserStorageItem(this.GetType().FriendlyName(), out json))
            {
                var uiContext = GetUIContext();
                deserializedDraft = uiContext.EntityService.GetObjectFromJson(typeof(TInput), json);
            }

            draft = (TInput)deserializedDraft;
            return (draft != null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected UIOperationContext GetUIContext()
        {
            var uiContext = UIOperationContext.Current;

            if (uiContext != null)
            {
                return uiContext;
            }

            throw new InvalidOperationException("Current operation was intended to be invoked by an interactive user");
        }
    }
}
