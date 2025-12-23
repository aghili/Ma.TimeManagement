using System.Runtime.Serialization;

namespace Ma.TimeManagement.Models
{
    [DataContract]
    public enum ProjectState
    {
        //
        // Summary:
        //     Project is in the process of being deleted.
        [EnumMember]
        Deleting = 2,
        //
        // Summary:
        //     Project is in the process of being created.
        [EnumMember]
        New = 0,
        //
        // Summary:
        //     Project is completely created and ready to use.
        [EnumMember]
        WellFormed = 1,
        //
        // Summary:
        //     Project has been queued for creation, but the process has not yet started.
        [EnumMember]
        CreatePending = 3,
        //
        // Summary:
        //     All projects regardless of state except Deleted.
        [EnumMember]
        All = -1,
        //
        // Summary:
        //     Project has not been changed.
        [EnumMember]
        Unchanged = -2,
        //
        // Summary:
        //     Project has been deleted.
        [EnumMember]
        Deleted = 4
    }
}