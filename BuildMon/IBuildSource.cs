using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildMon
{
    public interface IBuildDisplay 
    {
        void SetBuildItems(IEnumerable<IBuildItem> items);
        void AddBuildItems(IEnumerable<IBuildItem> items);
        void RemoveBuildItems(IEnumerable<string> items);
    }

    public interface IBuildConfig { }

    public enum BuildState {Success, Error, Building}
    public interface IBuildItem
    {
        string Id { get;}
        string ProjectName { get; }
        string ItemName {get;}
        bool IsFailure { get; set; }

        /// <summary>
        /// Build progress as a percentage to completion. Null means build is not currently running.
        /// </summary>
        int? ProgressPercentage { get; set; }
        event EventHandler<BuildStateChangedArgs> BuildStateChanged;
        event EventHandler<ProgressPercentageChangedArgs> ProgressPercentageChanged;
    }
    public interface IBuildSource
    {
        Task<IEnumerable<IBuildItem>> StartMonitoring(Action<IEnumerable<IBuildItem>> addedCallback, Action<IEnumerable<string>> removedCallback);
        //Task StartMonitoring();
        //void StopMonitoring();
    }
}
