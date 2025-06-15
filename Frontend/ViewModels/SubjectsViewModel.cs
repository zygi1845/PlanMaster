using Frontend.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace Frontend.ViewModels
{
    public class SubjectsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<SubjectModel> _subjects = new ObservableCollection<SubjectModel>();
        public ObservableCollection<SubjectModel> Subjects
        {
            get => _subjects;
            set
            {
                _subjects = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddSubjectCommand { get; }
        public ICommand RemoveSubjectCommand { get; }
        public ICommand SaveCommand { get; }

        public SubjectsViewModel()
        {
            LoadSubjects();

            AddSubjectCommand = new RelayCommand(_ => AddSubject());
            RemoveSubjectCommand = new RelayCommand<SubjectModel>(RemoveSubject);
            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
        }

        private void AddSubject()
        {
            Subjects.Add(new SubjectModel
            {
                Name = "",
                Hours = 0,                             // tutaj 0 zamiast -1
                SubjectType = SubjectTypes[0]         // domyślny typ z listy
            });
        }

        private void RemoveSubject(SubjectModel subject)
        {
            if (subject != null)
                Subjects.Remove(subject);
        }

        private void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(Subjects);
                File.WriteAllText("subjects.json", json);
            }
            catch (IOException)
            {
                // Obsługa błędu zapisu
            }
        }

        private bool CanSave()
        {
            // Tu możesz dodać walidację, np. sprawdzić puste nazwy
            return true;
        }

        private void LoadSubjects()
        {
            if (File.Exists("subjects.json"))
            {
                try
                {
                    var json = File.ReadAllText("subjects.json");
                    var loaded = JsonSerializer.Deserialize<ObservableCollection<SubjectModel>>(json);
                    if (loaded != null)
                        Subjects = loaded;
                }
                catch
                {
                    Subjects = new ObservableCollection<SubjectModel>();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}


public class RelayCommand : ICommand
{
    private readonly Action<object> execute;
    private readonly Predicate<object> canExecute;

    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
        this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this.canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => canExecute == null || canExecute(parameter);

    public void Execute(object parameter) => execute(parameter);

    public event EventHandler CanExecuteChanged
    {
        add    { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}

public class RelayCommand<T> : ICommand
{
    private readonly Action<T> execute;
    private readonly Func<T, bool> canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
    {
        this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this.canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => canExecute == null || canExecute((T)parameter);

    public void Execute(object parameter) => execute((T)parameter);

    public event EventHandler CanExecuteChanged
    {
        add    { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}
// using Frontend.Models;
// using System.Collections.ObjectModel;
// using System.ComponentModel;
// using System.Runtime.CompilerServices;
// using System.Windows.Input;

// namespace Frontend.ViewModels
// {
//     public class SubjectsViewModel : INotifyPropertyChanged
//     {
//         public ObservableCollection<SubjectModel> Subjects { get; set; }

//         public ICommand AddSubjectCommand { get; set; }
//         public ICommand SaveCommand { get; set; }

//         public SubjectsViewModel()
//         {
//             Subjects = new ObservableCollection<SubjectModel>();
//             AddSubjectCommand = new RelayCommand(AddSubject);
//             SaveCommand = new RelayCommand(SaveSubjects);
//         }

//         private void AddSubject()
//         {
//             Subjects.Add(new SubjectModel { Name = "", Hours = 0, SubjectType = "Wykłady" });
//         }

//         private void SaveSubjects()
//         {
//             // Logika zapisu — np. do pliku XML lub bazy danych
//         }

//         public event PropertyChangedEventHandler PropertyChanged;
//         private void OnPropertyChanged([CallerMemberName] string name = null)
//             => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
//     }
// }
