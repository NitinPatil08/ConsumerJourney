using System;

namespace ConsumerJourney.Entities
{
    public class ConsumerAppRecord
    {
        public string PackageName { get; set; }
        public string AppName { get; set; }
        public DateTime AppStartTime { get; set; }
        public DateTime AppEndTime { get; set; }
        public int AppCount { get; set; }
        public string AppCategory { get; set; }
    }
}