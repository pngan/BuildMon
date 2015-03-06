namespace BuildMon.Teamcity
{
    public class TeamcityBuild : TeamcityDescriptor
    {
        public int percentageComplete { get; set; }
        public string status { get; set; }
        public string buildTypeId { get; set; }
        public bool running { get; set; }
    }
}