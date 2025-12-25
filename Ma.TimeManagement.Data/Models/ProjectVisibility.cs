using System.Runtime.Serialization;

namespace Ma.TimeManagement.Models
{
    [DataContract]
    public enum ProjectVisibility
    {
        [EnumMember]
        Unchanged = -1,
        //
        // Summary:
        //     The project is only visible to users with explicit access.
        [EnumMember]
        Private,
        //
        // Summary:
        //     Enterprise level project visibility
        [EnumMember]
        Organization,
        //
        // Summary:
        //     The project is visible to all.
        [EnumMember]
        Public,
        [EnumMember]
        SystemPrivate
    }
}