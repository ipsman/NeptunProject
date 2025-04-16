using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using Ical.Net;
namespace NeptunProject.View;

public partial class Settings : ContentPage
{
    public ObservableCollection<DayTimetable> Days { get; set; }
    public Command<string> ChangeDayCommand { get; set; }
    public Command<TimetableEntry> ShowDetailsCommand { get; set; }
    private int _currentDayIndex;

    public Settings()
    {
        InitializeComponent();
    }

    
}