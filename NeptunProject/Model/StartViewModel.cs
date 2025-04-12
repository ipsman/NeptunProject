using System.ComponentModel;

namespace NeptunProject.Model
{
    public class StartViewModel : INotifyPropertyChanged
    {
        private string _userName;
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
        
        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
