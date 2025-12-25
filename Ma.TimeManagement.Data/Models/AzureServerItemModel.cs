using System.Runtime.Serialization;

namespace Ma.TimeManagement.Models
{
    [DataContract]
    public class AzureServerItemModel
    {
        [DataMember]
        public string ServerUrl { set; get; } = "http://cicd-server";

        [DataMember]
        public string Collection { set; get; } = "MahakSolutions";

        [DataMember]
        public string Project { set; get; }

        [DataMember]
        public string PAT { get; set; }
    }
}