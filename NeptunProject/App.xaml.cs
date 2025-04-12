using NeptunProject.View;

namespace NeptunProject
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            bool isFirstLaunch = Preferences.Get("FirstLaunch", true);

            if (isFirstLaunch)
            {
                Preferences.Set("FirstLaunch", false);
                Shell.Current.GoToAsync(nameof(StartView));
            }
        }
    }
}
