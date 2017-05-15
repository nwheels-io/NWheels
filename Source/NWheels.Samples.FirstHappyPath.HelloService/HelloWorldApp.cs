using NWheels.Contracts;
using NWheels.Frameworks.Uidl;
using NWheels.Frameworks.Uidl.Generic;
using NWheels.Frameworks.Uidl.Web;

namespace NWheels.Samples.FirstHappyPath.HelloService
{
    public class HelloWorldApp : WebApp<Empty.SessionState>
    {
        [DefaultPage]
        public class HomePage : WebPage<Empty.ViewModel>
        {
            [ContentElement]
            public DockLayoutElement Layout { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [NestedElement]
            public TransactionWizard<HelloWorldViewModel> Transaction { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void ConfigureElements()
            {
                Transaction.SubmitCommand.Label = "Go";                
                Layout.Fill.Add(Transaction);
            }

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
         
                [PropertyContract.ReadOnly, PropertyContract.Presentation.Label("WeSay")]
                public string Message { get; set; }
            }
        }
    }
}
