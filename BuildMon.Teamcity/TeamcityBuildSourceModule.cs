using Autofac;

namespace BuildMon.Teamcity
{
    public class TeamcityBuildSourceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TCBuildSource>().As<IBuildSource>().SingleInstance();
            builder.RegisterType<TCBuildConfig>().As<ITCBuildConfig, IBuildConfig>().SingleInstance();
            builder.RegisterType<TCBuildItem>().As<IBuildSourceItem>();
            builder.RegisterType<DllConfiguration>().As<IDllConfiguration>().SingleInstance();
        }
    }
}