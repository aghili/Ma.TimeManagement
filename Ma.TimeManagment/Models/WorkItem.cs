using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json.Converters;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ma.TimeManagement.Models
{
    public class WorkItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Title { get; set; }
        public EnWorkState State { get; internal set; }
        public double OriginalEstimate { get; internal set; }
        public double RemainingWork { get; set; }
        public double CompletedWork { get; set; }
        public string Url { get; internal set; }
        public EnWorkItemType WorkItemType { get; internal set; }

        public Guid ProjectID { get; set; }

        public double TotalWork { get => CompletedWork + RemainingWork; }
        public string ProjectName { get; internal set; }

        public TeamProjectReference ProjectReference { get; internal set; }
        public ICollection<WorkCalendarItem> WorkCalendarItems { get; internal set; }
    }
}