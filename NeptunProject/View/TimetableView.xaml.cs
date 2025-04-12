using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using Ical.Net;
namespace NeptunProject.View;

public partial class TimetableView : ContentPage
{
    public ObservableCollection<DayTimetable> Days { get; set; }
    public Command<string> ChangeDayCommand { get; set; }
    public Command<TimetableEntry> ShowDetailsCommand { get; set; }
    private int _currentDayIndex;

    public TimetableView()
    {
        InitializeComponent();
        ChangeDayCommand = new Command<string>(ChangeDay);
        ShowDetailsCommand = new Command<TimetableEntry>(ShowDetails);
        string jsonFilePath = Path.Combine(FileSystem.AppDataDirectory, "events.json");
        LoadJsonTimetableData(jsonFilePath);
    }

    private async void LoadJsonTimetableData(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.WriteLine("A megadott JSON fájl nem létezik.");
                return;
            }

            string json = await File.ReadAllTextAsync(filePath);
            var events = JsonSerializer.Deserialize<List<EventData>>(json) ?? new List<EventData>();

            // Napok sorrendjének biztosítása (Hétfõ -> Vasárnap)
            string[] dayOrder = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            Days = new ObservableCollection<DayTimetable>();

            var groupedEvents = events
                .GroupBy(e => e.StartTime.DayOfWeek.ToString())
                .OrderBy(g => Array.IndexOf(dayOrder, g.Key)) // Napok sorrendjét biztosítjuk
                .Select(g => new DayTimetable
                {
                    Day = g.Key,
                    Timetable = g.OrderBy(e => e.StartTime) // Idõ szerinti rendezés
                                 .Select(e => new TimetableEntry
                                 {
                                     Time = $"{e.StartTime:HH:mm} - {e.EndTime:HH:mm}",
                                     CourseName = e.Summary ?? "Nincs megadva",
                                     Location = e.Location ?? "Nincs megadva",
                                     Instructor = e.Description ?? "Nincs megadva"
                                 })
                                 .ToList()
                });

            foreach (var day in groupedEvents)
            {
                Days.Add(day);
            }

            if (Days != null && Days.Any()) // Ensure Days is not null and contains elements
            {
                int CurrentDay = ((int)DateTime.Now.DayOfWeek + 6) % 7; // 0 = Hétfõ, 6 = Vasárnap

                var foundItem = Days
                    .Select((day, idx) => new { day, idx })
                    .FirstOrDefault(x => x.day.Day == ((DayOfWeek)(CurrentDay + 1)).ToString());

                _currentDayIndex = foundItem != null ? foundItem.idx : 0; // Default to 0 if not found
            }
            else
            {
                _currentDayIndex = 0; // If the collection is empty, default to 0
            }

            BindingContext = this;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Hiba történt a JSON fájl beolvasása közben: {ex.Message}");
        }
    }


    private void ChangeDay(string direction)
    {
        if (direction == "Next" && _currentDayIndex < Days.Count - 1)
            _currentDayIndex++;
        else if (direction == "Previous" && _currentDayIndex > 0)
            _currentDayIndex--;

        OnPropertyChanged(nameof(CurrentDay));
    }

    private async void ShowDetails(TimetableEntry entry)
    {
        if (entry != null)
        {
            await Navigation.PushModalAsync(new DetailsModal(entry));
        }
    }

    public DayTimetable CurrentDay => Days[_currentDayIndex];
}

public class DayTimetable
{
    public string Day { get; set; }
    public List<TimetableEntry> Timetable { get; set; }
}

public class TimetableEntry
{
    public string Time { get; set; }
    public string CourseName { get; set; }
    public string Type { get; set; }
    public string Instructor { get; set; }
    public string Location { get; set; }
}