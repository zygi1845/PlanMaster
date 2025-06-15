using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Frontend.Models
{
    public class SubjectModel : INotifyPropertyChanged
    {
        private string name;
        private int hours;
        private string subjectType;

        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        public int Hours
        {
            get => hours;
            set { hours = value; OnPropertyChanged(); }
        }

        public string SubjectType
        {
            get => subjectType;
            set { subjectType = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
