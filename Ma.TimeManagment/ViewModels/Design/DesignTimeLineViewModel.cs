using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ma.TimeManagement.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Ma.TimeManagement.ViewModels.Design
{
    internal class TimeLineViewModel :ObservableObject, ITimeLineViewModel
    {
        public const int StartHour = 0;
        public const int EndHour = 24;
        public const int TotalHours = EndHour - StartHour;
        public const int TotalMinutes = TotalHours * 60;
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

        public ObservableCollection<TimelineItem> Items { get; set; } = [  new TimelineItem
            {
                Title = "Daily Standup",
                StartTime = DateTime.Today.AddHours(8),
                EndTime = DateTime.Today.AddHours(8).AddMinutes(15),
                Background = new SolidColorBrush(Colors.Orange)
            },new TimelineItem
            {
                Title = "Deep Work Session",
                StartTime = DateTime.Today.AddHours(9),
                EndTime = DateTime.Today.AddHours(12),
                Background = new SolidColorBrush(Colors.MediumSeaGreen)
            },new TimelineItem
            {
                Title = "Lunch & Walk",
                StartTime = DateTime.Today.AddHours(12).AddMinutes(30),
                EndTime = DateTime.Today.AddHours(13).AddMinutes(45),
                Background = new SolidColorBrush(Colors.Gray)
            },new TimelineItem
            {
                Title = "Code Review",
                StartTime = DateTime.Today.AddHours(14),
                EndTime = DateTime.Today.AddHours(15).AddMinutes(30),
                Background = new SolidColorBrush(Colors.IndianRed)
            },new TimelineItem
            {
                Title = "Team Sync",
                StartTime = DateTime.Today.AddHours(17),
                EndTime = DateTime.Today.AddHours(18),
                Background = new SolidColorBrush(Colors.RoyalBlue)
            }];
        public double Zoom { get; set; } = 80;

        public IRelayCommand ZoomInCommand { get; }

        public IRelayCommand ZoomOutCommand { get; }

        public IRelayCommand ResetCommand { get; }

        public IRelayCommand StartTaskCommand { get; }
        public IRelayCommand InsertTaskCommand { get; }
    }
}
