using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Ma.TimeManagement.Models
{
    [DataContract]
    public class TeamProjectReference
    {
        //
        // Summary:
        //     Project identifier.
        [DataMember(Order = 0, EmitDefaultValue = false)]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        //
        // Summary:
        //     Project abbreviation.
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string? Abbreviation { get; set; }

        //
        // Summary:
        //     Project name.
        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string Name { get; set; }

        //
        // Summary:
        //     The project's description (if any).
        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string? Description { get; set; }

        //
        // Summary:
        //     Url to the full version of the object.
        [DataMember(Order = 4, EmitDefaultValue = false)]
        public string Url { get; set; } = "";

        //
        // Summary:
        //     Project state.
        [DataMember(Order = 5)]
        public ProjectState State { get; set; }

        //
        // Summary:
        //     Project revision.
        [DataMember(Order = 6, EmitDefaultValue = false)]
        public long Revision { get; set; }

        //
        // Summary:
        //     Project visibility.
        [DataMember(Order = 7)]
        public ProjectVisibility Visibility { get; set; }

        //
        // Summary:
        //     Url to default team identity image.
        [DataMember(Order = 8, EmitDefaultValue = false)]
        public string? DefaultTeamImageUrl { get; set; }

        //
        // Summary:
        //     Project last update time.
        [DataMember(Order = 9)]
        public DateTime LastUpdateTime { get; set; }

        public Guid NamespaceId { get; set; }

        public int RequiredPermissions { set; get; }

        public string? Token { set; get; }

        public ICollection<WorkItem> workItems { get; set; }
    }
}