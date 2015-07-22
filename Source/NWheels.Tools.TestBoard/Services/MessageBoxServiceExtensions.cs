using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Tools.TestBoard.Services
{
    public static class MessageBoxServiceExtensions
    {
        public static void InternalConcurrencyErrorEncountered(this IMessageBoxService messageBox)
        {
            messageBox.ErrorOK("An internal concurrency error encountered. Please try again.");
        }
    }
}
