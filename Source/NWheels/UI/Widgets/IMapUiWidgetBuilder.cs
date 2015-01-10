using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.Modules.Geo;

namespace NWheels.UI.Widgets
{
    public interface IMapUiWidgetBuilder<TModel, TState> : IBoundUiElementBuilder<TModel, TState, IMapUiWidgetBuilder<TModel, TState>>
    {
        IMapUiWidgetBuilder<IWidgetMapUiState, TState> BindTo(Expression<Func<IUiScope<TModel, TState>, IWidgetMapUiState>> path);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWidgetMapUiState
    {
        IEntityPartGeoCoordinate Center { get; set; }
        int RadiusInMeters { get; set; }
    }
}
