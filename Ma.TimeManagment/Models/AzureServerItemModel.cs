using System.Runtime.Serialization;

namespace Ma.TimeManagement.Models
{
    [DataContract]
    public class AzureServerItemModel
    {
        [DataMember]
        public string ServerUrl { set; get; } = "https://cicd-server";

        [DataMember]
        public string Collection { set; get; } = "DefaultCollection";

        [DataMember]
        public string Project { set; get; }

        [DataMember]
        public string PAT { get; set; }
    }
}