using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NWheels.Globalization.Core;
using NWheels.Samples.MyMusicDB.Domain;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Samples.MyMusicDB.UI
{
    public class MusicDBDashboardScreenPart: ScreenPartBase<MusicDBDashboardScreenPart, Empty.Input, Empty.Data, Empty.State>
    {
        public MusicDBDashboardScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Dashboard Dashboard { get; set; }
        public Gauge Genres { get; set; }
        public Gauge Artists { get; set; }
        public Gauge Albums { get; set; }
        public Gauge Tracks { get; set; }
        public Gauge Users { get; set; }
        public Gauge ApiRequests { get; set; }
        public Chart ApiRequestsOverTime { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<DashboardQueryTx.IOutput> DataReceived { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<MusicDBDashboardScreenPart, Empty.Data, Empty.State> presenter)
        {
            this.ContentRoot = Dashboard;
            this.Dashboard.AddWidgets(Genres, Artists, Albums, Tracks, Users, ApiRequests);
            this.Dashboard.AddWidgets(ApiRequestsOverTime);

            this.Genres.BindToModelSetter(DataReceived).Value(v => v.Data(property: x => x.Genres).Format("#,##0"));
            this.Artists.BindToModelSetter(DataReceived).Value(v => v.Data(property: x => x.Artists).Format("#,##0"));
            this.Albums.BindToModelSetter(DataReceived).Value(v => v.Data(property: x => x.Albums).Format("#,##0"));
            this.Tracks.BindToModelSetter(DataReceived).Value(v => v.Data(property: x => x.Tracks).Format("#,##0"));
            this.Users.BindToModelSetter(DataReceived).Value(v => v.Data(property: x => x.Users).Format("#,##0"));
            this.ApiRequests.BindToModelSetter(DataReceived).Value(v => v.Data(property: x => x.ApiRequests).Format("#,##0"));
            this.ApiRequests.Text = "APIRequests";
            this.ApiRequestsOverTime.BindToModelSetter(DataReceived, x => x.ApiRequestsOverTime);
            this.ApiRequestsOverTime.Height = WidgetSize.Medium;

            presenter.On(NavigatedHere)
                .InvokeTransactionScript<DashboardQueryTx>()
                .WaitForReply((tx, vm) => tx.Execute(null))
                .Then(b => b.Broadcast(DataReceived).WithPayload(vm => vm.Input).TunnelDown());

            ApiRequestsOverTime.Text = "APIRequestsOverPast48Hours";
        }
    }
}
