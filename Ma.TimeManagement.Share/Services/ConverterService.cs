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

        public WorkItemDto ConvertTo(Guid ProjectID,Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem Item)
        {
            bool isValid = Validate(Item,out List<string> fields);
            object? value;
            return new()
            {
                Id = Item.Id ?? 0,
                ProjectID = ProjectID,
                ProjectName =  Convert.ToString(Item.Fields["System.TeamProject"]) ?? "",
                Url = Item.Url,
                Title = (isValid ? "" : $"[FIELD MISSING({string.Join(",",fields)})]") + Convert.ToString(Item.Fields["System.Title"]) ?? "",
                State = Enum.Parse<EnWorkState>(Convert.ToString(Item.Fields["System.State"]) ?? "", true),
                WorkItemType = Enum.Parse<EnWorkItemType>(Convert.ToString(Item.Fields["System.WorkItemType"]) ?? ""),
                OriginalEstimate = Item.Fields.TryGetValue("Microsoft.VSTS.Scheduling.OriginalEstimate", out value) ?Convert.ToDouble(value ?? "0.0"):0,
                RemainingWork = Item.Fields.TryGetValue("Microsoft.VSTS.Scheduling.RemainingWork", out value) ? Convert.ToDouble(value ?? "0.0"):0,
                CompletedWork = Item.Fields.TryGetValue("Microsoft.VSTS.Scheduling.CompletedWork", out value) ? Convert.ToDouble(value ?? "0.0"):0
            };
        }
        public WorkItem ConvertTo(WorkItemDto Item)
        {
            return new()
            {
                Id = Item.Id,
                ProjectName = Item.ProjectName,
                Url = Item.Url,
                Title = Item.Title,
                State = Item.State,
                WorkItemType = Item.WorkItemType,
                OriginalEstimate = Item.OriginalEstimate,
                RemainingWork = Item.RemainingWork,
                CompletedWork = Item.CompletedWork
            };
        }

        private bool Validate(Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem item, out List<string> fields)
        {
            fields = [];
            if (!item.Fields.ContainsKey("Microsoft.VSTS.Scheduling.OriginalEstimate"))
                fields.Add("OriginalEstimate");
            if (!item.Fields.ContainsKey("Microsoft.VSTS.Scheduling.RemainingWork"))
                fields.Add("RemainingWork");
            if (!item.Fields.ContainsKey("Microsoft.VSTS.Scheduling.CompletedWork"))
                     fields.Add("CompletedWork");
            return fields.Count == 0;
        }

        public Models.TeamProjectReferenceDto ConvertTo(Microsoft.TeamFoundation.Core.WebApi.TeamProjectReference Item)
        {
            Enum.TryParse(Item.State.ToString(), true, out ProjectState state);
            Enum.TryParse(Item.Visibility.ToString(), true, out ProjectVisibility visibility);

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
                State = state,
                Visibility = visibility
            };
        }
        public TeamProjectReference ConvertTo(TeamProjectReferenceDto Item)
        {
            Enum.TryParse(Item.State.ToString(), true, out ProjectState state);
            Enum.TryParse(Item.Visibility.ToString(), true, out ProjectVisibility visibility);

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
                State = state,
                Visibility = visibility
            };
        }

        public IEnumerable<WorkItemDto> ConvertTo(Guid ProjectID,IEnumerable<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem> Items)
        {
            List<WorkItemDto> items = [];
            foreach (var workItem in Items)
                items.Add(ConvertTo(ProjectID,workItem));
            return items;
        }
        public IEnumerable<WorkItem> ConvertTo(IEnumerable<WorkItemDto> Items)
        {
            List<WorkItem> items = [];
            foreach (var workItem in Items)
                items.Add(ConvertTo(workItem));
            return items;
        }

        public IEnumerable<TeamProjectReferenceDto> ConvertTo(IEnumerable<Microsoft.TeamFoundation.Core.WebApi.TeamProjectReference> Items)
        {
            List<TeamProjectReferenceDto> items = [];
            foreach (var Item in Items)
                items.Add(ConvertTo(Item));
            return items;
        }
        public IEnumerable<TeamProjectReference> ConvertTo(IEnumerable<TeamProjectReferenceDto> Items)
        {
            List<TeamProjectReference> items = [];
            foreach (var Item in Items)
                items.Add(ConvertTo(Item));
            return items;
        }

        public WorkItemAddDto ConvertTo(WorkItemDto workItem,string discution)
        {
            return new() { CompletedWork = workItem.CompletedWork, OriginalEstimate = workItem.OriginalEstimate, RemainingWork = workItem.RemainingWork, ProjectID = workItem.ProjectID, State = workItem.State, Title = workItem.Title, WorkItemType = workItem.WorkItemType,Discution = discution };
        }
    }
}