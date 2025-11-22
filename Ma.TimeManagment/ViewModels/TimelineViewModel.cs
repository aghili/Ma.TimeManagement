// TimelineViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ma.TimeManagement.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

namespace Ma.TimeManagement.ViewModels
{
    public partial class TimelineViewModel : ObservableObject
    {
        public const int StartHour = 8;
        public const int EndHour = 22;
        public const int TotalHours = EndHour - StartHour;
        public const int TotalMinutes = TotalHours * 60;

        [ObservableProperty]
        private ObservableCollection<TimelineItem> _items = new();
        [ObservableProperty] private double _zoom = 80; // pixels per hour

        public DateTime DayStart => DateTime.Today.AddHours(StartHour);
        public DateTime DayEnd => DateTime.Today.AddHours(EndHour);

        public double TimelineWidth
        {
            get
            {
                return
                     TotalMinutes * (Zoom / 60.0);
            }
        }

        public TimelineViewModel()
        {
            ZoomInCommand = new RelayCommand(() => Zoom = Math.Min(Zoom * 1.4, 400));
            ZoomOutCommand = new RelayCommand(() => Zoom = Math.Max(Zoom / 1.4, 20));
            ResetCommand = new RelayCommand(() => Zoom = 80);

            // Fixed: Correct way to add hours + minutes
            Items.Add(new TimelineItem
            {
                Title = "Daily Standup",
                StartTime = DateTime.Today.AddHours(8),
                EndTime = DateTime.Today.AddHours(8).AddMinutes(15),
                Background = new SolidColorBrush(Colors.Orange)
            });

            Items.Add(new TimelineItem
            {
                Title = "Deep Work Session",
                StartTime = DateTime.Today.AddHours(9),
                EndTime = DateTime.Today.AddHours(12),
                Background = new SolidColorBrush(Colors.MediumSeaGreen)
            });

            Items.Add(new TimelineItem
            {
                Title = "Lunch & Walk",
                StartTime = DateTime.Today.AddHours(12).AddMinutes(30),
                EndTime = DateTime.Today.AddHours(13).AddMinutes(45),
                Background = new SolidColorBrush(Colors.Gray)
            });

            Items.Add(new TimelineItem
            {
                Title = "Code Review",
                StartTime = DateTime.Today.AddHours(14),
                EndTime = DateTime.Today.AddHours(15).AddMinutes(30),
                Background = new SolidColorBrush(Colors.IndianRed)
            });

            Items.Add(new TimelineItem
            {
                Title = "Team Sync",
                StartTime = DateTime.Today.AddHours(17),
                EndTime = DateTime.Today.AddHours(18),
                Background = new SolidColorBrush(Colors.RoyalBlue)
            });
        }

        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand ResetCommand { get; }
    }
}