using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Modules.Geo;

namespace NWheels.UI.Widgets
{
    public interface IMapUiWidgetBuilder<TModel, TState> : IBoundUiElementBuilder<TModel, TState>
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWidgetMapUiState
    {
        IEntityPartGeoCoordinate Center { get; set; }
        int RadiusInMeters { get; set; }
    }
}
