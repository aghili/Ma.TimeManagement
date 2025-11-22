using Hardcodet.Wpf.TaskbarNotification;

namespace Ma.TimeManagement.Models
{
    public class StatusModel(BalloonIcon icon, string? description, string? title = null)
    {
        public BalloonIcon Icon { get; set; } = icon;
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
        Message
    }
}