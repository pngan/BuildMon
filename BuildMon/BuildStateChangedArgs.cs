using System;

namespace BuildMon
{
    public class BuildStateChangedArgs : EventArgs
    {
        public BuildStateChangedArgs(bool isFailure)
        {
            IsFailure = isFailure;
        }

        public bool IsFailure { get; private set; }
    }
}