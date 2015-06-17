using Hapil;

namespace NWheels.UI.Core
{
    public abstract class UINodeDescription
    {
        protected UINodeDescription(string idName, UINodeDescription parent)
        {
            this.NodeType = UINodeType.NotSet;
            this.IdName = idName;
            this.Parent = parent;
            this.QualifiedName = parent.QualifiedName + "_" + idName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UINodeType NodeType { get; protected set; }
        public string IdName { get; private set; }
        public string QualifiedName { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UINodeDescription Parent { get; private set; }
    }
}
