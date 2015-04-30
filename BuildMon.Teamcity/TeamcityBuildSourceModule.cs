using Autofac;

namespace BuildMon.Teamcity
{
    public class TeamcityBuildSourceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TcBuildSource>().As<IBuildSource>().SingleInstance();
            builder.RegisterType<TcBuildConfig>().As<ITcBuildConfig, IBuildConfig>().SingleInstance();
            builder.RegisterType<TcBuildItem>().As<IBuildSourceItem>();
            builder.RegisterType<DllConfiguration>().As<IDllConfiguration>().SingleInstance();
        }
    }
}