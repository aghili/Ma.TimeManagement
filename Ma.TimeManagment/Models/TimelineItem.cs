using System.Windows.Media;

namespace Ma.TimeManagement.Models
{
    public class TimelineItem
    {
        public string Title { get; set; } = "Untitled";
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public object? Tag { get; set; }
        public Brush Background { get; set; } = new SolidColorBrush(Colors.CornflowerBlue);
        public int TaskId { get; internal set; }
        public string ProjectName { get; internal set; }
    }
}
