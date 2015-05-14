using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Tools.DevFlow.Parsers
{
    internal static class PegExtensions
    {
        public static VsSlnNodeType VsSlnNodeType(this PegNode node)
        {
            return (VsSlnNodeType)node.id_;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static PegNode[] SubNodes(this PegNode node, VsSlnNodeType nodeType, bool recursive = true)
        {
            var foundNodes = new List<PegNode>();
            FindSubNodes(node, nodeType, foundNodes, recursive);
            return foundNodes.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void FindSubNodes(PegNode parentNode, VsSlnNodeType nodeType, List<PegNode> foundNodes, bool recursive)
        {
            for ( var childNode = parentNode.child_ ; childNode != null ; childNode = childNode.next_ )
            {
                if ( childNode.VsSlnNodeType() == nodeType )
                {
                    foundNodes.Add(childNode);
                }

                if ( recursive )
                {
                    FindSubNodes(childNode, nodeType, foundNodes, recursive: true);
                }
            }
        }
    }
}
