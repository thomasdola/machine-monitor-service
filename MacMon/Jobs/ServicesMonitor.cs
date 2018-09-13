using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using MacMon.Models;
using Phoenix;
using Quartz;

namespace MacMon.Jobs
{
    public class ServicesMonitor : IJob
    {
        private static void Watch(Process process, Channel channel)
        {
            var sc = GetService(process.Name);
            if (sc == null) return;
            int running;
            if  (sc.Status.Equals(ServiceControllerStatus.Stopped) ||
                 sc.Status.Equals(ServiceControllerStatus.StopPending))
            {
                running = 0;
            }  
            else
            {
                running = 1;
            }  
                
            var report = new Dictionary<string, object>
            {
                {"status", running},
                {"name", process.Name},
                {"path", process.Path}
            };

            if (channel.canPush)
            {
                channel.Push(Services.WebSocket.MacMonWebSocket.ServiceStatusChanged, report);
            }
        }

        private static ServiceController GetService(string name)
        {
            return ServiceController.GetServices()
                .FirstOrDefault(s => s.ServiceName == name);
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var channel = (Channel)context.JobDetail.JobDataMap["channel"];
            var services = (List<Process>)context.JobDetail.JobDataMap["services"];
            Console.WriteLine("services -> {0} on channel -> {1}", services.Count, channel.topic);
            foreach (var service in services)
            {
                Watch(service, channel);
            }
        }
    }
}