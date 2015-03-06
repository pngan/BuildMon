using System;

namespace BuildMon
{
    public interface IBuildItem
    {
        string Id { get; }
        string ProjectName { get; }
        string ItemName { get; }
        bool IsFailure { get; set; }

        /// <summary>
        ///     Build progress as a percentage to completion. Null means build is not currently running.
        /// </summary>
        int? ProgressPercentage { get; set; }

        event EventHandler<BuildStateChangedArgs> BuildStateChanged;
        event EventHandler<ProgressPercentageChangedArgs> ProgressPercentageChanged;
    }
}