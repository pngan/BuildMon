using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Resources;
using System.Windows;
using Autofac;
using System.Reflection;
using BuildMon.Logging;
using Serilog;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace BuildMon
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IContainer _container;


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var teamcityAssembly = Assembly.LoadFile(Assembly.GetEntryAssembly().Location + @"\..\BuildMon.Teamcity.dll");
            var wpfDisplayAssembly = Assembly.LoadFile(Assembly.GetEntryAssembly().Location + @"\..\BuildMon.Display.WPF.dll");

            ReadViewsFromPlugins(wpfDisplayAssembly);
            BuildIoCContainerFromPlugins(teamcityAssembly, wpfDisplayAssembly);
        }

        private void BuildIoCContainerFromPlugins(Assembly teamcityAssembly, Assembly wpfDisplayAssembly)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();

            var builder = new ContainerBuilder();
            builder.Register(c => SerilogConfig.CreateLogger())
               .As<ILogger>()
               .SingleInstance();
            builder.RegisterModule<SerilogLoggingModule>(); 
            builder.RegisterAssemblyTypes(thisAssembly).AsImplementedInterfaces();
            builder.RegisterAssemblyModules(teamcityAssembly);
            builder.RegisterAssemblyModules(wpfDisplayAssembly);
            builder.RegisterType<MainWindow>();
            builder.RegisterType<MainViewModel>();

            _container = builder.Build();
        }

        private void ReadViewsFromPlugins(Assembly pluginAssembly)
        {
            var stream = pluginAssembly.GetManifestResourceStream(pluginAssembly.GetName().Name + ".g.resources");
            var resourceReader = new ResourceReader(stream);

            foreach (DictionaryEntry resource in resourceReader)
            {
                if (new FileInfo(resource.Key.ToString()).Extension.Equals(".baml"))
                {
                    var uri = new Uri("/" + pluginAssembly.GetName().Name + ";component/" + resource.Key.ToString().Replace(".baml", ".xaml"), UriKind.Relative);
                    var pluginView = Application.LoadComponent(uri) as ResourceDictionary;
                    this.Resources.MergedDictionaries.Add(pluginView);
                }
            }
        }
    }
}
