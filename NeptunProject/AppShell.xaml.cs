using NeptunProject.View;

namespace NeptunProject
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(StartView), typeof(StartView));
            Routing.RegisterRoute(nameof(TimetableView), typeof(TimetableView));
            Routing.RegisterRoute(nameof(Subjects), typeof(Subjects));
        }
    }
}
