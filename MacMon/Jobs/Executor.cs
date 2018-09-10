using System;
using Castle.Core.Internal;
using MacMon.Models;
using Phoenix;
using Quartz;

namespace MacMon.Jobs
{
    public class Executor
    {
        private readonly Channel _channel;

        private Executor(Channel channel)
        {
            _channel = channel;
        }

        public static Executor Init(Channel channel)
        {
            return new Executor(channel);
        }

        public void Start(Job job)
        {
            Console.WriteLine("job | apps -> {} | services -> {}", job.Applications.Count, job.Services.Count);
            if (job.Network)
            {
                var trigger = TriggerBuilder.Create().WithIdentity("Network Trigger", "Triggers").StartNow()
                    .WithSimpleSchedule(s => s
                        .WithIntervalInSeconds(5)
                        .RepeatForever())
                    .Build();
                
                var networkJob = JobBuilder.Create<NetworkMonitor>().WithIdentity("Network Job", "Jobs").Build();
                
                networkJob.JobDataMap["channel"] = _channel;
                Scheduler.Quartz.Start(networkJob, trigger);
            }

            else if (!job.Applications.IsNullOrEmpty())
            {
                var trigger = TriggerBuilder.Create().WithIdentity("Applications Trigger", "Triggers").StartNow()
                    .WithSimpleSchedule(s => s
                        .WithIntervalInSeconds(5)
                        .RepeatForever())
                    .Build();

                var applicationsJob = JobBuilder.Create<ApplicationsMonitor>().WithIdentity("Applications Job", "Jobs")
                    .Build();

                applicationsJob.JobDataMap["channel"] = _channel;
                applicationsJob.JobDataMap["applications"] = job.Applications;
                Scheduler.Quartz.Start(applicationsJob, trigger);
            }

            else if (!job.Services.IsNullOrEmpty())
            {
                var trigger = TriggerBuilder.Create().WithIdentity("Services Trigger", "Triggers").StartNow()
                    .WithSimpleSchedule(s => s
                        .WithIntervalInSeconds(5)
                        .RepeatForever())
                    .Build();

                var servicesJob = JobBuilder.Create<ServicesMonitor>().WithIdentity("Services Job", "Jobs")
                    .Build();

                servicesJob.JobDataMap["channel"] = _channel;
                servicesJob.JobDataMap["services"] = job.Services;
                Scheduler.Quartz.Start(servicesJob, trigger);
            }
        }
    }
}