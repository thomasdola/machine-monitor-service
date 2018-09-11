using System;
using Castle.Core.Internal;
using JsonFlatFileDataStore;
using MacMon.Models;
using Phoenix;
using Quartz;

namespace MacMon.Jobs
{
    public class Executor
    {
        private readonly Channel _channel;
        private readonly DataStore _store;

        private UserActivitiesMonitor _userActivitiesMonitor;
        private LocationMonitor _locationMonitor;

        private Executor(Channel channel, DataStore store)
        {
            _channel = channel;
            _store = store;
        }

        public static Executor Init(Channel channel, DataStore store)
        {
            return new Executor(channel, store);
        }

        public void Start(Job job)
        {
            Console.WriteLine("job | apps -> {} | services -> {}", job.Applications.Count, job.Services.Count);
         
            _userActivitiesMonitor = new UserActivitiesMonitor(_store, _channel);
            _userActivitiesMonitor.OnStart();
            
            _locationMonitor = new  LocationMonitor(_store, _channel);
            _locationMonitor.OnStart();
            
            StartNetworkMonitor();

            if (!job.Applications.IsNullOrEmpty())
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

        private void StartNetworkMonitor()
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

        public void Stop()
        {
            _userActivitiesMonitor.OnStop();
            _locationMonitor.OnStop();
        }
    }
}