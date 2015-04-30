// ReSharper disable InconsistentNaming
namespace BuildMon.Teamcity
{
    public class TeamcityBuildType : TeamcityDescriptor
    {
        public string projectName { get; set; }
        public string projectId { get; set; }
        public string webUrl { get; set; }
    }
}