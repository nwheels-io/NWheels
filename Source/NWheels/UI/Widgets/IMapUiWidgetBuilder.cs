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
        IMapUiWidgetBuilder<TModel, TState> BindToModel(Expression<Func<TModel, IWidgetMapUiState>> path);
        IMapUiWidgetBuilder<TModel, TState> BindToUiState(Expression<Func<TState, IWidgetMapUiState>> path);
        IMapUiWidgetBuilder<TModel, TState> Initialize<T>(Expression<Func<IWidgetMapUiState, T>> path, T value);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWidgetMapUiState
    {
        IEntityPartGeoCoordinate Center { get; set; }
        int RadiusInMeters { get; set; }
    }
}
