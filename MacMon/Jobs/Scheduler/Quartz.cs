using System.Collections.Specialized;
using Quartz;
using Quartz.Impl;

namespace MacMon.Jobs.Scheduler
{
    public static class Quartz
    {
        public static async void Start(IJobDetail job, ITrigger trigger)
        {
            var props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            };
            var factory = new StdSchedulerFactory(props);
            
            var scheduler = await factory.GetScheduler();
            await scheduler.Start();
            
            await scheduler.ScheduleJob(job, trigger);
        }
    }
}