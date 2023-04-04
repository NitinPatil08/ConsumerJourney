using System;
using System.Collections.Generic;

namespace ConsumerJourney.Entities
{
    public class AppRecord
    {
        public int Id { get; set; }
        public string PackageName { get; set; }
        public string AppName { get; set; }
        public AppType Type { get; set; }
        public DateTime AppStartTime { get; set; }
        public DateTime AppEndTime { get; set; }
        public string AppCategory { get; set; }
        public bool IsCurrentApp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PreviousApp { get; set; }
        public string NextApp { get; set; }
        public List<AppData> Data { get; set; }
    }
}