using System.Runtime.Serialization;

namespace Ma.TimeManagement.Models
{
    [DataContract]
    public class WorkItemDto
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public EnWorkState State { get; set; }
        [DataMember]
        public double OriginalEstimate { get; set; }
        [DataMember]
        public double RemainingWork { get; set; }
        [DataMember]
        public double CompletedWork { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public EnWorkItemType WorkItemType { get; set; }
        [DataMember]
        public Guid ProjectID { get; set; }
        [DataMember]
        public double TotalWork { get => CompletedWork + RemainingWork; }
        [DataMember]
        public string ProjectName { get; set; }
    }
}