namespace NeptunProject.View;
using Microsoft.Maui.Controls;

public partial class DetailsModal : ContentPage
{
    public Command CloseCommand { get; }

    public DetailsModal(TimetableEntry entry)
    {
        InitializeComponent();

        CloseCommand = new Command(async () => await CloseModal());

        BindingContext = new DetailsModalViewModel(entry, CloseCommand);
    }

    private async Task CloseModal()
    {
        await Navigation.PopModalAsync();
    }
}

public class DetailsModalViewModel
{
    public TimetableEntry Entry { get; }
    public Command CloseCommand { get; }

    public DetailsModalViewModel(TimetableEntry entry, Command closeCommand)
    {
        Entry = entry;
        CloseCommand = closeCommand;
    }

    public string Time => Entry.Time;
    public string CourseName => Entry.CourseName;
    public string Type => Entry.Type;
    public string Instructor => Entry.Instructor;
}