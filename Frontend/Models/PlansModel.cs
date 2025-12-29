using System.IO;
using System.Text.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Frontend.Models
{
    public class PlansModel : INotifyPropertyChanged
    {
        public string godzina { get; set; }
        public string przedmiot { get; set; }
        public string dzien { get; set; }
        public string nauczyciel { get; set; }
        public string sala { get; set; }
        public string klasa { get; set; }

        private bool _hasConflict;
        public bool HasConflict
        {
            get => _hasConflict;
            set
            {
                _hasConflict = value;
                OnPropertyChanged(nameof(HasConflict));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}