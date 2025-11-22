using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Ma.TimeManagement.Models
{
    [DataContract]
    public class WorkCalendarItem
    {
        [Key]
        public DateTime Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? WorkItemID { get; set; }

        public WorkItem? WorkItem { get; set; }
    }
}