using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Runtime.Serialization;

namespace Ma.TimeManagement.Models
{
    [DataContract]
    public class WorkCalendarItem
    {
        [Key]
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public double DurationHour { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public int? WorkItemID { get; set; }

        public WorkItem? WorkItem { get; set; }
        public bool Synced { get; set; }
    }
}