using NeptunProject.Model;
using System.Text.Json;

namespace NeptunProject.View
{
    public partial class StartView : ContentPage
    {
        private StartViewModel _viewModel;
        public StartView()
        {
            InitializeComponent();
            //_viewModel = new StartViewModel();
            //BindingContext = _viewModel;
        }

        private async void PickAndNavigate(object sender, EventArgs e)
        {
            var result = await FilePicker.PickAsync();
            if (result != null && Path.GetExtension(result.FullPath).ToLower() == ".ics")
            {
                Preferences.Set("IcsFilePath", result.FullPath);

                var events = await ParseIcsFile(result.FullPath);

                string localPath = FileSystem.AppDataDirectory;
                
                string eventsJson = JsonSerializer.Serialize(events);
                await File.WriteAllTextAsync(Path.Combine(localPath, "events.json"), eventsJson);

                string subjectsJson = JsonSerializer.Serialize(subjects);
                await File.WriteAllTextAsync(Path.Combine(localPath, "subjects.json"), subjectsJson);

                Preferences.Set("Name", Name.Text);
                //Preferences.Set("Semester", _viewModel.SemesterValue.ToString());

                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                await DisplayAlert("Hibás fájl", "Kérlek válassz egy érvényes ICS fájlt!", "OK");
            }
        }

        //private async Task<List<EventData>> ParseIcsFile(string filePath)
        //{
        //    var events = new List<EventData>();
        //    string[] lines = await File.ReadAllLinesAsync(filePath);
        //    EventData currentEvent = null;
        //    foreach (string line in lines)
        //    {
        //        if (line.StartsWith("BEGIN:VEVENT"))
        //        {
        //            currentEvent = new EventData();
        //        }
        //        else if (line.StartsWith("END:VEVENT") && currentEvent != null)
        //        {
        //            events.Add(currentEvent);
        //            currentEvent = null;
        //        }
        //        else if (currentEvent != null)
        //        {
        //            if (line.StartsWith("SUMMARY:"))
        //            {
        //                currentEvent.Summary = line.Substring(8);
        //            }
        //            else if (line.StartsWith("DTSTART:"))
        //            {
        //                string dateTimeString = line.Substring(8);
        //                currentEvent.StartTime = DateTime.ParseExact(dateTimeString, "yyyyMMddTHHmmssZ",
        //                    System.Globalization.CultureInfo.InvariantCulture,
        //                    System.Globalization.DateTimeStyles.AssumeUniversal);
        //            }
        //            else if (line.StartsWith("DTEND:"))
        //            {
        //                string dateTimeString = line.Substring(6);
        //                currentEvent.EndTime = DateTime.ParseExact(dateTimeString, "yyyyMMddTHHmmssZ",
        //                    System.Globalization.CultureInfo.InvariantCulture,
        //                    System.Globalization.DateTimeStyles.AssumeUniversal);
        //            }
        //            else if (line.StartsWith("LOCATION:"))
        //            {
        //                currentEvent.Location = line.Substring(9);
        //            }
        //        }
        //    }
        //    return events;
        //}
        List<string> subjects;

        private async Task<List<EventData>> ParseIcsFile(string filePath)
        {
            var events = new List<EventData>();
            string[] lines = await File.ReadAllLinesAsync(filePath);
            EventData currentEvent = null;
            subjects = new List<string>();


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

                        if (subjects.Count == 0)
                        {
                            subjects.Add(line.Substring(8).Split("(")[0]);
                        }
                        else if(subjects[subjects.Count - 1] != line.Substring(8).Split("(")[0] && subjects.Count > 0)
                        {
                            subjects.Add(line.Substring(8).Split("(")[0]);
                        }
                        
                    }
                    else if (line.StartsWith("DTSTART:"))
                    {
                        string dateTimeString = line.Substring(8);
                        currentEvent.StartTime = DateTime.ParseExact(dateTimeString, "yyyyMMddTHHmmssZ",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.AssumeUniversal);
                    }
                    else if (line.StartsWith("DTEND:"))
                    {
                        string dateTimeString = line.Substring(6);
                        currentEvent.EndTime = DateTime.ParseExact(dateTimeString, "yyyyMMddTHHmmssZ",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.AssumeUniversal);
                    }
                    else if (line.StartsWith("LOCATION:"))
                    {
                        currentEvent.Location = line.Substring(9);
                    }
                }
            }
            return events;
        }
    }

    public class EventData
    {
        public string Summary { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; }
        public string? Description { get; internal set; }
    }
}
