using System.Collections.ObjectModel;
using System.ComponentModel;
using INZYNIERKA.Frontend.Models;

namespace INZYNIERKA.Frontend.ViewModels
{
    public class ClasesViewModel : INotifyPropertyChanged
    {
        private int _iloscTypow;
        private int _iloscKlas;

        public int IloscTypow
        {
            get => _iloscTypow;
            set
            {
                _iloscTypow = value;
                OnPropertyChanged(nameof(IloscTypow));
            }
        }

        public int IloscKlas
        {
            get => _iloscKlas;
            set
            {
                _iloscKlas = value;
                OnPropertyChanged(nameof(IloscKlas));
            }
        }

        public ObservableCollection<ClassTypeModel> TypyKlas { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
