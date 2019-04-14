using System.Collections.Generic;

namespace NWheels.UI.Adapters.Web.Wix
{
    public class WixComponentDefinition
    {
        public string Type { get; } = "Component";
        public string ComponentType { get; set; }
        public string Skin { get; set; }
        public string Style { get; set; }
        public WixComponentLayout Layout { get; set; }
        public WixComponentConnections Connections { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public Dictionary<string, object> Props { get; set; }
        public Dictionary<string, object> ActiveModes { get; set; }
    }

    public class WixComponentLayout
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Scale { get; set; } = 1;
        public int RotationInDegrees { get; set; }
        public bool FixedPosition { get; set; }
    }

    public class WixComponentConnections
    {
        public string Type { get; set; } = "ConnectionList";
        public List<WixComponentConnectionItem> Items { get; set; }
    }

    public class WixComponentConnectionItem
    {
        public string Type { get; set; }
        public string Role { get; set; }
    }
}
