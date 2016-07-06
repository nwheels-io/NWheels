using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Globalization.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class LanguageSelector : WidgetBase<LanguageSelector, Empty.Data, Empty.State>
    {
        public LanguageSelector(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<LanguageSelector, Empty.Data, Empty.State> presenter)
        {
            presenter.On(Select)
                .ProjectInputAs<ChangeSessionLanguageTx.IInput>(alt => alt.Copy(vm => vm.Input.Source).To(vm => vm.Input.Target.IsoCode))
                .Then(b1 => b1
                    .InvokeTransactionScript<ChangeSessionLanguageTx>()
                    .WaitForCompletion((tx, vm) => tx.Execute(vm.Input.Target))
                    .Then(
                        b2 => b2.RestartApp()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand<string> Select { get; set; }
    }
}
