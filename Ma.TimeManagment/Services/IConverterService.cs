





namespace Ma.TimeManagement.Services
{
    public interface IConverterService
    {
        Models.WorkItem ConvertTo(Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem Item);
        Models.TeamProjectReference ConvertTo(Microsoft.TeamFoundation.Core.WebApi.TeamProjectReference Item);
        IEnumerable<Models.WorkItem> ConvertTo(IEnumerable<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem> Items);
        IEnumerable<Models.TeamProjectReference> ConvertTo(IEnumerable<Microsoft.TeamFoundation.Core.WebApi.TeamProjectReference> Items);
    }
}