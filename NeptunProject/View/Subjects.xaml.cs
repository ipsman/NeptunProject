using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using Microsoft.Maui.Controls;

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
            CalculateAverage(); // Calculate average when the list changes
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
        BindingContext = this; // Set BindingContext here, after SubjectsList is initialized
    }

    private async void LoadSubjects(string filePath)
    {
        var tempSubjects = new ObservableCollection<Subject>();

        try
        {
            if (File.Exists(filePath))
            {
                string json = await File.ReadAllTextAsync(filePath);
                var subjectNames = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();

                foreach (var subjectName in subjectNames)
                {
                    var newSubject = new Subject { Name = subjectName };
                    newSubject.ResultChanged += CalculateAverage; // Subscribe to the ResultChanged event
                    tempSubjects.Add(newSubject);
                }
            }
            else
            {
                Debug.WriteLine("A megadott JSON f�jl nem l�tezik. �res list�t hozunk l�tre.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Hiba t�rt�nt a JSON f�jl beolvas�sa k�zben: {ex.Message}");
        }

        SubjectsList = tempSubjects; // Assign the loaded subjects to the bound property
        CalculateAverage(); // Calculate initial average
    }

    private void CalculateAverage()
    {
        var validSubjects = SubjectsList.Where(subject => subject.Result.HasValue).ToList();
        Average = validSubjects.Any() ? (float)validSubjects.Average(subject => subject.Result.Value) : 0;
    }

    private async void EditGrade(Subject subject)
    {
        if (subject != null)
        {
            string gradeInput = await DisplayPromptAsync(
                "Jegy m�dos�t�sa",
                $"Adja meg az �rdemjegyet a(z) {subject.Name} t�rgyhoz (1-5):",
                "Ment�s",
                "M�gse",
                keyboard: Keyboard.Numeric);

            if (int.TryParse(gradeInput, out int newGrade) && newGrade >= 1 && newGrade <= 5)
            {
                subject.Result = newGrade; // Setting the Result will trigger the PropertyChanged event in Subject, and subsequently CalculateAverage
            }
            else if (!string.IsNullOrEmpty(gradeInput))
            {
                await DisplayAlert("Hiba", "K�rj�k, �rv�nyes jegyet adjon meg (1-5).", "OK");
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
                ResultChanged?.Invoke(); // Notify that the Result has changed
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

// It seems like SubjectManager isn't directly used in the provided code snippet for updating the average.
// If you intend to manage subjects globally, you would need to integrate it into the Subjects page.
// For the current requirement of updating the average on grade change within the Subjects page,
// the existing logic in the Subjects.cs and Subject.cs is sufficient.
// I'm leaving the SubjectManager class here as it was in your original code.
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