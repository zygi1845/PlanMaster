using Frontend.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Frontend.ViewModels
{
    public class SubjectsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<SubjectModel> _allSubjects = new ObservableCollection<SubjectModel>();
        private ObservableCollection<SubjectModel> _filteredSubjects = new ObservableCollection<SubjectModel>();
        private ObservableCollection<ClassModel> _classTypes = new ObservableCollection<ClassModel>();
        private string _selectedClassType;
        
        public ObservableCollection<SubjectModel> FilteredSubjects
        {
            get => _filteredSubjects;
            set
            {
                _filteredSubjects = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ClassModel> ClassTypes
        {
            get => _classTypes;
            set
            {
                _classTypes = value;
                OnPropertyChanged();
            }
        }

        public string SelectedClassType
        {
            get => _selectedClassType;
            set
            {
                _selectedClassType = value;
                OnPropertyChanged();
                FilterSubjectsByClassType();
            }
        }

        // Lista typów zajęć do bindowania w ComboBox
        public List<string> SubjectTypes { get; } = new List<string>
        {
            "Pracownia Specjalistyczna",
            "Wykłady",
            "Ćwiczenia"
        };

        public ICommand AddSubjectCommand { get; }
        public ICommand RemoveSubjectCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SelectClassTypeCommand { get; }

        public SubjectsViewModel()
        {
            LoadClassTypes();
            LoadSubjects();

            AddSubjectCommand = new RelayCommand(_ => AddSubject());
            RemoveSubjectCommand = new RelayCommand<SubjectModel>(RemoveSubject);
            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            SelectClassTypeCommand = new RelayCommand<string>(SelectClassType);

            // Ustaw domyślny typ klasy jeśli są dostępne
            if (ClassTypes.Count > 0)
            {
                SelectedClassType = ClassTypes[0].NazwaTypu;
            }
        }

        private void SelectClassType(string classType)
        {
            SelectedClassType = classType;
        }

        private void LoadClassTypes()
        {
            try
            {
                if (File.Exists("class.json"))
                {
                    var json = File.ReadAllText("class.json");
                    var classes = JsonSerializer.Deserialize<ObservableCollection<ClassModel>>(json);
                    
                    if (classes != null)
                    {
                        ClassTypes = new ObservableCollection<ClassModel>(classes);
                    }
                    else
                    {
                        ClassTypes = new ObservableCollection<ClassModel>();
                    }
                }
                else
                {
                    ClassTypes = new ObservableCollection<ClassModel>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd wczytywania klas: {ex.Message}");
                ClassTypes = new ObservableCollection<ClassModel>();
            }
        }

        private void FilterSubjectsByClassType()
        {
            if (string.IsNullOrEmpty(SelectedClassType))
            {
                FilteredSubjects = new ObservableCollection<SubjectModel>(_allSubjects);
                return;
            }

            var filtered = _allSubjects.Where(subject => 
                subject.ClassTypes == null || 
                subject.ClassTypes.Contains(SelectedClassType) ||
                subject.ClassTypes.Count == 0
            ).ToList();

            FilteredSubjects = new ObservableCollection<SubjectModel>(filtered);
        }

        private void AddSubject()
        {
            var newSubject = new SubjectModel
            {
                Name = "",
                Hours = 0,
                SubjectType = SubjectTypes[0],
                ClassTypes = new List<string>()
            };

            // Automatycznie dodaj do aktualnie wybranego typu klasy
            if (!string.IsNullOrEmpty(SelectedClassType))
            {
                newSubject.ClassTypes.Add(SelectedClassType);
            }

            _allSubjects.Add(newSubject);
            FilterSubjectsByClassType();
        }

        private void RemoveSubject(SubjectModel subject)
        {
            if (subject != null)
            {
                _allSubjects.Remove(subject);
                FilterSubjectsByClassType();
            }
        }

        private void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(_allSubjects);
                File.WriteAllText("subjects.json", json);
                MessageBox.Show("Zapisano pomyślnie!");
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Błąd zapisu: {ex.Message}");
            }
        }

        private bool CanSave()
        {
            return true;
        }

        private void LoadSubjects()
        {
            try
            {
                if (File.Exists("subjects.json"))
                {
                    var json = File.ReadAllText("subjects.json");
                    var loaded = JsonSerializer.Deserialize<ObservableCollection<SubjectModel>>(json);
                    if (loaded != null)
                    {
                        _allSubjects = loaded;
                        FilterSubjectsByClassType();
                    }
                }
                else
                {
                    _allSubjects = new ObservableCollection<SubjectModel>();
                    FilteredSubjects = new ObservableCollection<SubjectModel>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd wczytywania przedmiotów: {ex.Message}");
                _allSubjects = new ObservableCollection<SubjectModel>();
                FilteredSubjects = new ObservableCollection<SubjectModel>();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

// using Frontend.Models;
// using System.Collections.ObjectModel;
// using System.ComponentModel;
// using System.Runtime.CompilerServices;
// using System.Windows.Input;
// using System;
// using System.IO;
// using System.Text.Json;
// using System.Collections.Generic;

// namespace Frontend.ViewModels
// {
//     public class SubjectsViewModel : INotifyPropertyChanged
//     {
//         private ObservableCollection<SubjectModel> _subjects = new ObservableCollection<SubjectModel>();
//         public ObservableCollection<SubjectModel> Subjects
//         {
//             get => _subjects;
//             set
//             {
//                 _subjects = value;
//                 OnPropertyChanged();
//             }
//         }

//         // Lista typów zajęć do bindowania w ComboBox
//         public List<string> SubjectTypes { get; } = new List<string>
//         {
//             "Pracownia Specjalistyczna",
//             "Wykłady",
//             "Ćwiczenia"
//         };

//         public ICommand AddSubjectCommand { get; }
//         public ICommand RemoveSubjectCommand { get; }
//         public ICommand SaveCommand { get; }

//         public SubjectsViewModel()
//         {
//             LoadSubjects();

//             AddSubjectCommand = new RelayCommand(_ => AddSubject());
//             RemoveSubjectCommand = new RelayCommand<SubjectModel>(RemoveSubject);
//             SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
//         }

//         private void AddSubject()
//         {
//             Subjects.Add(new SubjectModel
//             {
//                 Name = "",
//                 Hours = 0,                             // tutaj 0 zamiast -1
//                 SubjectType = SubjectTypes[0]         // domyślny typ z listy
//             });
//         }

//         private void RemoveSubject(SubjectModel subject)
//         {
//             if (subject != null)
//                 Subjects.Remove(subject);
//         }

//         private void Save()
//         {
//             try
//             {
//                 var json = JsonSerializer.Serialize(Subjects);
//                 File.WriteAllText("subjects.json", json);
//             }
//             catch (IOException)
//             {
//                 // Obsługa błędu zapisu
//             }
//         }

//         private bool CanSave()
//         {
//             // Tu możesz dodać walidację, np. sprawdzić puste nazwy
//             return true;
//         }

//         private void LoadSubjects()
//         {
//             if (File.Exists("subjects.json"))
//             {
//                 try
//                 {
//                     var json = File.ReadAllText("subjects.json");
//                     var loaded = JsonSerializer.Deserialize<ObservableCollection<SubjectModel>>(json);
//                     if (loaded != null)
//                         Subjects = loaded;
//                 }
//                 catch
//                 {
//                     Subjects = new ObservableCollection<SubjectModel>();
//                 }
//             }
//         }

//         public event PropertyChangedEventHandler PropertyChanged;
//         private void OnPropertyChanged([CallerMemberName] string name = null)
//             => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
//     }
// }


// public class RelayCommand : ICommand
// {
//     private readonly Action<object> execute;
//     private readonly Predicate<object> canExecute;

//     public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
//     {
//         this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
//         this.canExecute = canExecute;
//     }

//     public bool CanExecute(object parameter) => canExecute == null || canExecute(parameter);

//     public void Execute(object parameter) => execute(parameter);

//     public event EventHandler CanExecuteChanged
//     {
//         add    { CommandManager.RequerySuggested += value; }
//         remove { CommandManager.RequerySuggested -= value; }
//     }
// }

// public class RelayCommand<T> : ICommand
// {
//     private readonly Action<T> execute;
//     private readonly Func<T, bool> canExecute;

//     public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
//     {
//         this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
//         this.canExecute = canExecute;
//     }

//     public bool CanExecute(object parameter) => canExecute == null || canExecute((T)parameter);

//     public void Execute(object parameter) => execute((T)parameter);

//     public event EventHandler CanExecuteChanged
//     {
//         add    { CommandManager.RequerySuggested += value; }
//         remove { CommandManager.RequerySuggested -= value; }
//     }
// }