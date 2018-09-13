using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MacMon.Models;
using Phoenix;
using Quartz;

namespace MacMon.Jobs
{
    public class ApplicationsMonitor : IJob
    {
        private static void Watch(Process process, Channel channel)
        {
            var appsByName = System.Diagnostics.Process.GetProcessesByName(process.Name);
            foreach (var app in appsByName)
            {
                if (app == null) continue;
                var running = app.HasExited ? 0 : 1;
                
                var report = new Dictionary<string, object>
                {
                    {"status", running},
                    {"name", process.Name},
                    {"path", process.Path}    
                };

                if (channel.canPush)
                {
                    channel.Push(Services.WebSocket.MacMonWebSocket.ApplicationStatusChanged, report);
                }
            }
        }
        
        public async Task Execute(IJobExecutionContext context)
        {
            var channel = (Channel)context.JobDetail.JobDataMap["channel"];
            var applications = (List<Process>)context.JobDetail.JobDataMap["applications"];
            Console.WriteLine("applications -> {0} on channel -> {1}", applications.Count, channel.topic);
            foreach (var application in applications)
            {
                Watch(application, channel);
            }
        }
    }
}