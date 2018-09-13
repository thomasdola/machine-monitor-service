using Topshelf;

namespace MacMon
{
    internal static class ConfigureMacMon
    {
        internal static void Configure()
        {

            var server = App.DefaultServer;
        
            HostFactory.Run(configure =>  
            {  
                configure.Service < App > (service =>  
                {  
                    service.ConstructUsing(s => new App(server));  
                    service.WhenStarted(s => s.Start());  
                    service.WhenStopped(s => s.Stop());  
                });  
                //Setup Account that window service use to run.  
                configure.RunAsLocalSystem();  
                
                configure.UseAssemblyInfoForServiceInfo();
                
                configure.SetServiceName("MacMon");  
                configure.SetDisplayName("Machine Monitor");  
                configure.SetDescription("Service to monitor this machine.");

                configure.StartAutomatically();
                
                configure.AddCommandLineDefinition("server", v => server = v);
            });  
        }
    }
}