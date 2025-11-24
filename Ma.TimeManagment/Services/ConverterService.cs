using Ma.TimeManagement.Models;

namespace Ma.TimeManagement.Services
{
    public class ConverterService : IConverterService
    {
        public ConverterService() { }

        public double ConvertHourToRounded(double hour)
        {
            return Math.Round((hour) * 4, MidpointRounding.ToPositiveInfinity) / 4;
        }

        public WorkItem ConvertTo(Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem Item)
        {
            return new()
            {
                Id = Item.Id??0,
                ProjectName = Convert.ToString(Item.Fields["System.TeamProject"]) ?? "",
                Url = Item.Url,
                Title = Convert.ToString(Item.Fields["System.Title"]) ?? "",
                State = Enum.Parse<EnWorkState>(Convert.ToString(Item.Fields["System.State"])?? "",true),
                WorkItemType = Enum.Parse<EnWorkItemType>(Convert.ToString(Item.Fields["System.WorkItemType"]) ?? ""),
                OriginalEstimate = Convert.ToDouble(Item.Fields["Microsoft.VSTS.Scheduling.OriginalEstimate"] ?? "0.0"),
                RemainingWork = Convert.ToDouble(Item.Fields["Microsoft.VSTS.Scheduling.RemainingWork"] ?? "0.0"),
                CompletedWork = Convert.ToDouble(Item.Fields["Microsoft.VSTS.Scheduling.CompletedWork"] ?? "0.0")
            };
        }

        public Models.TeamProjectReference ConvertTo(Microsoft.TeamFoundation.Core.WebApi.TeamProjectReference Item)
        {
            return new()
            {
                Id = Item.Id,
                Url = Item.Url,
                Abbreviation = Item.Abbreviation,
                DefaultTeamImageUrl = Item.DefaultTeamImageUrl,
                Description = Item.Description,
                LastUpdateTime = Item.LastUpdateTime,
                Name = Item.Name,
                Revision = Item.Revision,
                State = Item.State,
                Visibility = Item.Visibility
            };
        }

        public IEnumerable<WorkItem> ConvertTo(IEnumerable<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem> Items)
        {
            List<WorkItem> items = [];
            foreach (var workItem in Items)
                items.Add(ConvertTo(workItem));
            return items;
        }

        public IEnumerable<Models.TeamProjectReference> ConvertTo(IEnumerable<Microsoft.TeamFoundation.Core.WebApi.TeamProjectReference> Items)
        {
            List<TeamProjectReference> items = [];
            foreach (var Item in Items)
                items.Add(ConvertTo(Item));
            return items;
        }
    }
}