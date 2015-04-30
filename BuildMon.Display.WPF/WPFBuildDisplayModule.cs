using Autofac;

namespace BuildMon.Display.WPF
{
    public class WpfBuildDisplayModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BuildDisplay>().As<IBuildDisplay>().SingleInstance();
            builder.RegisterType<DisplayBuildItem>().As<IBuildDisplayItem>();
        }
    }
}