using Autofac;

namespace BuildMon.Display.WPF
{
    public class WPFBuildDisplayModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BuildDisplay>().As<IBuildDisplay>().SingleInstance();
            builder.RegisterType<DisplayBuildItem>().As<IBuildDisplayItem>();
        }
    }
}