using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;

namespace NeptunProject.View;

public partial class Subjects : ContentPage, INotifyPropertyChanged
{
    private float _average;
    private ObservableCollection<Subject> _subjects;

    public ObservableCollection<Subject> SubjectsList
    {
        get => _subjects;
        set
        {
            _subjects = value;
            OnPropertyChanged();
        }
    }

    public ICommand EditGradeCommand { get; }

    public float Average
    {
        get => _average;
        private set
        {
            if (_average != value)
            {
                _average = value;
                OnPropertyChanged();
            }
        }
    }

    public Subjects()
    {
        InitializeComponent();
        SubjectsList = new ObservableCollection<Subject>();
        EditGradeCommand = new Command<Subject>(EditGrade);
        string jsonFilePath = Path.Combine(FileSystem.AppDataDirectory, "subjects.json");
        LoadSubjects(jsonFilePath);
    }

    private async void LoadSubjects(string filePath)
    {
        //var tempSubjects = new List<Subject>
        //{
        //    //new Subject { Name = "Operációkutatás és döntéselmélet", Result = 4 },
        //    //new Subject { Name = "Informatika projekt 1.", Result = 3 },
        //    //new Subject { Name = "Numerikus módszerek", Result = 4 },
        //    //new Subject { Name = "Web programozás", Result = 4 },
        //    //new Subject { Name = "Mérés- és irányítástechnika", Result = 4 },
        //};

        //foreach (var subject in tempSubjects)
        //{
        //    subject.ResultChanged += CalculateAverage;
        //    SubjectsList.Add(subject);
        //}

        //CalculateAverage();

        var tempSubjects = new List<Subject>();

        try
        {
            if (!File.Exists(filePath))
            {
                Debug.WriteLine("A megadott JSON fájl nem létezik.");
                return;
            }

            string json = await File.ReadAllTextAsync(filePath);
            var events = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();

            foreach (var subject in events)
            {
                tempSubjects.Add(new Subject { Name = subject });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Hiba történt a JSON fájl beolvasása közben: {ex.Message}");
        }


        foreach (var subject in tempSubjects)
        {
            //subject.ResultChanged += CalculateAverage;
            SubjectsList.Add(subject);
        }

        BindingContext = this;
    }

    //private void CalculateAverage()
    //{
    //    var validSubjects = SubjectsList.Where(subject => subject.Result.HasValue).ToList();
    //    Average = validSubjects.Any() ? (float)validSubjects.Average(subject => subject.Result.Value) : 0;
    //}

    private async void EditGrade(Subject subject)
    {
        if (subject != null)
        {
            string gradeInput = await DisplayPromptAsync(
                "Jegy módosítása",
                $"Adja meg az érdemjegyet a(z) {subject.Name} tárgyhoz (1-5):",
                "Mentés",
                "Mégse",
                keyboard: Keyboard.Numeric);

            if (int.TryParse(gradeInput, out int newGrade) && newGrade >= 1 && newGrade <= 5)
            {
                subject.Result = newGrade;
            }
            else if (!string.IsNullOrEmpty(gradeInput))
            {
                await DisplayAlert("Hiba", "Kérjük, érvényes jegyet adjon meg (1-5).", "OK");
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Subject : INotifyPropertyChanged
{
    private int? _result;
    public string Name { get; set; }

    public int? Result
    {
        get => _result;
        set
        {
            if (_result != value)
            {
                _result = value;
                OnPropertyChanged();
                ResultChanged?.Invoke();
            }
        }
    }

    public event Action ResultChanged;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class SubjectManager : INotifyPropertyChanged
{
    private static SubjectManager _instance;
    public static SubjectManager Instance => _instance ??= new SubjectManager();

    private ObservableCollection<Subject> _subjects;
    public ObservableCollection<Subject> Subjects
    {
        get => _subjects;
        set
        {
            if (_subjects != value)
            {
                _subjects = value;
                OnPropertyChanged();
            }
        }
    }

    private SubjectManager()
    {
        Subjects = new ObservableCollection<Subject>();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
