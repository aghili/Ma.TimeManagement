using System.Runtime.Serialization;

namespace Ma.TimeManagement.Models
{
    [DataContract]
    public class WorkItemUpdateDto
    {
        [DataMember]
        public string? Title { get; set; }
        [DataMember]
        public EnWorkState? State { get; set; }
        [DataMember]
        public double? OriginalEstimate { get; set; }
        [DataMember]
        public double? RemainingWork { get; set; }
        [DataMember]
        public double? CompletedWork { get; set; }
        [DataMember]
        public string? Discution { get; set; }
    }
}