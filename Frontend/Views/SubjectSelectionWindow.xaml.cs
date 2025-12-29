using Frontend.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Frontend.Views
{
    public partial class SubjectSelectionWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<SubjectModel> _availableSubjects;
        private SubjectModel _selectedSubject;

        public ObservableCollection<SubjectModel> AvailableSubjects
        {
            get => _availableSubjects;
            set
            {
                _availableSubjects = value;
                OnPropertyChanged();
            }
        }

        public SubjectModel SelectedSubject
        {
            get => _selectedSubject;
            set
            {
                _selectedSubject = value;
                OnPropertyChanged();
            }
        }

        public SubjectSelectionWindow(List<SubjectModel> allSubjects, List<SubjectModel> alreadyTaughtSubjects)
        {
            InitializeComponent();
            DataContext = this;

            // Pokaż tylko te przedmioty które nauczyciel jeszcze nie uczy
            var available = allSubjects.Where(s => 
                !alreadyTaughtSubjects.Any(taught => taught.Name == s.Name)
            ).ToList();

            AvailableSubjects = new ObservableCollection<SubjectModel>(available);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSubject != null)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(
                    "⚠️ Wybierz przedmiot z listy!",
                    "Brak wyboru",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}