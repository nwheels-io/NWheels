using NWheels.Contracts;
using NWheels.Frameworks.Uidl;
using NWheels.Frameworks.Uidl.Generic;
using NWheels.Frameworks.Uidl.Web;

namespace NWheels.Samples.FirstHappyPath.HelloService
{
    [WebAppComponent]
    public class HelloWorldApp : WebApp<Empty.SessionState>
    {
        [DefaultPage]
        public class HomePage : WebPage<Empty.ViewModel>
        {
            [ContentElement]
            [TransactonWizard.Configure(SubmitCommandLabel = "Go")]
            public TransactionWizard<HelloWorldViewModel> Transaction { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void ImplementController()
            {
                Transaction.OnSubmitCallTx<HelloWorldTx>(
                    tx => tx.Hello(Transaction.Model.Name)
                )
                .Then(
                    result => Script.Assign(Transaction.Model.Message, result)
                );
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class HelloWorldViewModel 
            {
                [PropertyContract.Required]
                public string Name { get; set; }
         
                [PropertyContract.Semantics.Output, PropertyContract.Presentation.Label("WeSay")]
                public string Message { get; set; }
            }
        }
    }
}
