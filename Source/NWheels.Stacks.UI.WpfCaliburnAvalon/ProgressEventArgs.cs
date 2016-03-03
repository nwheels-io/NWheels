using System;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(Progress progress)
        {
            Progress = progress;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Progress Progress { get; private set; }
    }
}