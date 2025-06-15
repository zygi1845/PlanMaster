using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Frontend.Models
{
    public class TeacherModel : INotifyPropertyChanged
    {
        private int nr_pracownika;
        private string nazwisko;

        public int Nr_pracownika
        {
            get => nr_pracownika;
            set { nr_pracownika = value; OnPropertyChanged(); }
        }

        public string Nazwisko
        {
            get => nazwisko;
            set { nazwisko = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string nazwisko = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nazwisko));
    }
}
