using System.Collections.Specialized;
using Quartz;
using Quartz.Impl;

namespace MacMon.Jobs.Scheduler
{
    public static class Quartz
    {
        public static async void Start(IJobDetail job, ITrigger trigger)
        {
            NameValueCollection props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            
            IScheduler sched = await factory.GetScheduler();
            await sched.Start();
            
            await sched.ScheduleJob(job, trigger);
        }
    }
}