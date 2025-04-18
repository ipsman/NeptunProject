using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Maui.Storage;

namespace NeptunProject.View;

public partial class Settings : ContentPage
{
    private List<string> _subjects = new List<string>();

    private double _semesterValue = 1;
    public double SemesterValue
    {
        get => _semesterValue;
        set
        {
            if (_semesterValue != value)
            {
                _semesterValue = value;
                OnPropertyChanged(nameof(SemesterValue));
            }
        }
    }

    public Settings()
    {
        InitializeComponent();

        Name.Text = Preferences.Get("Name", string.Empty);


        string savedSemester = Preferences.Get("Semester", "1");
        if (int.TryParse(savedSemester, out int semester))
        {
            _semesterValue = semester;
        }
        BindingContext = this; // Ha a XAML-ben binding-et használsz a code-behind tulajdonságaihoz
    }

    private async void OnPickFileClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "text/calendar", "application/octet-stream" } },
                { DevicePlatform.iOS, new[] { "public.calendar-event" } },
                { DevicePlatform.WinUI, new[] { ".ics" } },
                { DevicePlatform.Tizen, new[] { "*/*" } },
                { DevicePlatform.macOS, new[] { "public.calendar-event" } },
            })
        });

        if (result != null && Path.GetExtension(result.FullPath).ToLower() == ".ics")
        {
            Preferences.Set("IcsFilePath", result.FullPath);
            var events = await ParseIcsFile(result.FullPath);

            string localPath = FileSystem.AppDataDirectory;
            string eventsJson = JsonSerializer.Serialize(events);
            await File.WriteAllTextAsync(Path.Combine(localPath, "events.json"), eventsJson);

            string subjectsJson = JsonSerializer.Serialize(_subjects.Distinct().ToList()); // Csak az egyedi tantárgyakat mentjük
            await File.WriteAllTextAsync(Path.Combine(localPath, "subjects.json"), subjectsJson);

            await DisplayAlert("Siker", "Órarend importálva és tantárgyak mentve!", "OK");
        }
        else
        {
            await DisplayAlert("Hibás fájl", "Kérlek válassz egy érvényes ICS fájlt!", "OK");
        }
    }

    private async void OnSaveSettingsClicked(object sender, EventArgs e)
    {
        Preferences.Set("Name", Name.Text);
        // Ha a félévszámot a Settings oldalon kezeled, akkor itt mentheted:
        Preferences.Set("Semester", SemesterValue.ToString());

        await DisplayAlert("Mentés", "A beállítások elmentve!", "OK");
    }

    private async Task<List<EventData>> ParseIcsFile(string filePath)
    {
        var events = new List<EventData>();
        string[] lines = await File.ReadAllLinesAsync(filePath);
        EventData currentEvent = null;

        foreach (string line in lines)
        {
            if (line.StartsWith("BEGIN:VEVENT"))
            {
                currentEvent = new EventData();
            }
            else if (line.StartsWith("END:VEVENT") && currentEvent != null)
            {
                events.Add(currentEvent);
                currentEvent = null;
            }
            else if (currentEvent != null)
            {
                if (line.StartsWith("SUMMARY:"))
                {
                    currentEvent.Summary = line.Substring(8);
                    string subjectName = line.Substring(8).Split('(')[0].Trim();
                    if (!_subjects.Contains(subjectName))
                    {
                        _subjects.Add(subjectName);
                    }
                }
                else if (line.StartsWith("DTSTART:"))
                {
                    string dateTimeString = line.Substring(8);
                    if (DateTime.TryParseExact(dateTimeString, "yyyyMMddTHHmmssZ", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime startTime))
                    {
                        currentEvent.StartTime = startTime;
                    }
                }
                else if (line.StartsWith("DTEND:"))
                {
                    string dateTimeString = line.Substring(6);
                    if (DateTime.TryParseExact(dateTimeString, "yyyyMMddTHHmmssZ", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime endTime))
                    {
                        currentEvent.EndTime = endTime;
                    }
                }
                else if (line.StartsWith("LOCATION:"))
                {
                    currentEvent.Location = line.Substring(9);
                }
                else if (line.StartsWith("DESCRIPTION:"))
                {
                    currentEvent.Description = line.Substring(12);
                }
            }
        }
        return events;
    }
}