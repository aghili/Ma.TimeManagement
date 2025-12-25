





using Ma.TimeManagement.Models;

namespace Ma.TimeManagement.Services
{
    public interface IConverterService
    {
        double ConvertHourToRounded(double hour);
        WorkItemDto ConvertTo(Guid ProjectID, Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem Item);
        WorkItem ConvertTo(WorkItemDto Item);
        TeamProjectReferenceDto ConvertTo(Microsoft.TeamFoundation.Core.WebApi.TeamProjectReference Item);
        TeamProjectReference ConvertTo(TeamProjectReferenceDto Item);
        IEnumerable<WorkItemDto> ConvertTo(Guid ProjectID, IEnumerable<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem> Items);
        IEnumerable<WorkItem> ConvertTo(IEnumerable<WorkItemDto> Items);
        IEnumerable<TeamProjectReferenceDto> ConvertTo(IEnumerable<Microsoft.TeamFoundation.Core.WebApi.TeamProjectReference> Items);
        IEnumerable<TeamProjectReference> ConvertTo(IEnumerable<TeamProjectReferenceDto> Items);
        WorkItemAddDto ConvertTo(WorkItemDto workItem, string discution);
    }
}