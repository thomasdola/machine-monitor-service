using System.Collections.Generic;

namespace MacMon.Models
{
    public class Job
    {
        public List<Process> Applications { set; get; }
        public List<Process> Services { set; get; }
    }
}