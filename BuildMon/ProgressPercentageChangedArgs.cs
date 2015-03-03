using System;

namespace BuildMon
{
    public class ProgressPercentageChangedArgs : EventArgs
    {
        public ProgressPercentageChangedArgs(int? percentageProgress)
        {
            PercentageProgress = percentageProgress;
        }

        public int? PercentageProgress { get; private set; }
    }
}