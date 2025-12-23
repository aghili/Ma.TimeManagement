namespace Ma.TimeManagement.Models
{
    public class StatusModel(EnBalloonIcon icon, string? description, string? title = null)
    {
        public EnBalloonIcon Icon { get; set; } = icon;
        public string? Title { get; set; } = title;
        public string? Description { get; set; } = description;
    }

    public class StatusActionModel(EnStatusAction action)
    {
        public EnStatusAction Action { get; set; } = action;
    }

    public enum EnStatusAction
    {
        RefreshTasks,
        Message,
        RefreshItem
    }
}