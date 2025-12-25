using System.Runtime.Serialization;

namespace Ma.TimeManagement.Models
{
    [DataContract]
    public class SettingGeneralModel
    {
        [DataMember]
        public List<AzureServerItemModel> Servers { set; get; } = [new()];
        [DataMember]
        public bool BypassProxyOnLocal { get; set; }
    }
}