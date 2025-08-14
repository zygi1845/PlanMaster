using Frontend.Models;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Frontend.ViewModels
{
    public class GenerationViewModel : INotifyPropertyChanged
    {
        private readonly GenerationModel _model = new GenerationModel();

        public int TeacherCount => _model.TeacherCount;
        public int RoomCount => _model.RoomCount;
        public int ClassCount => _model.ClassCount;
        public int ClassTypeCount => _model.ClassTypeCount;

        // Statyczne komendy RoutedUICommand
        public static RoutedUICommand GenerateCommand { get; } = 
            new RoutedUICommand("Generuj plan", "GenerateCommand", typeof(GenerationViewModel));
        
        public static RoutedUICommand NavigateCommand { get; } = 
            new RoutedUICommand("Nawiguj", "NavigateCommand", typeof(GenerationViewModel));

        public GenerationViewModel()
        {
            _model.LoadData();
        }

        public void GenerateTimetable()
        {
            MessageBox.Show($"Generowanie planu:\nNauczyciele: {TeacherCount}\nSale: {RoomCount}\nKlasy: {ClassCount}\nTypy klas: {ClassTypeCount}");
        }

        public void Navigate(string destination)
        {
            MessageBox.Show($"Nawigacja do: {destination}");
            // Tutaj implementacja zmiany okien
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}