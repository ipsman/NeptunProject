using NeptunProject.View;



namespace NeptunProject
{
    public partial class MainPage : ContentPage
    {
        public Command NavigateToTimetableCommand { get; set; }
        public Command NavigateToSubjectsCommand { get; set; }

        public MainPage()
        {
            InitializeComponent();

            NavigateToSubjectsCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync(nameof(Subjects));
            });

            NavigateToTimetableCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync(nameof(TimetableView));
            });


            WelcomeLabel.Text = $"Welcome back, {Preferences.Get("Name", "Name")}!";

            SemesterLabel.Text = $"You in your {Preferences.Get("Semester", 1)}th semester!";

            BindingContext = this;
        }        
    }
}
