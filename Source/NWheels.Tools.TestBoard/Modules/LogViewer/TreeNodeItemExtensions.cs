using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.Tools.TestBoard.Modules.LogViewer
{
    public static class TreeNodeItemExtensions
    {
        public static TreeNodeItem<ThreadLogViewModel.NodeItem> ExpandToRootCauseError(this TreeNodeItem<ThreadLogViewModel.NodeItem> parentNode)
        {
            if ( parentNode.Data.NodeKind.IsIn(LogNodeKind.ActivityFailure, LogNodeKind.ThreadFailure) )
            {
                parentNode.IsExpanded = true;

                for ( var childNode = parentNode.FirstChildNode ; childNode != null ; childNode = childNode.NextSiblingNode )
                {
                    if ( childNode.Data.IsErrorNode )
                    {
                        if ( childNode.Data.NodeKind == LogNodeKind.ActivityFailure )
                        {
                            var rootCauseNode = ExpandToRootCauseError(childNode);

                            if ( rootCauseNode != null )
                            {
                                return rootCauseNode;
                            }
                        }

                        return childNode;
                    }
                }
            }

            return parentNode;
        }
    }
}
