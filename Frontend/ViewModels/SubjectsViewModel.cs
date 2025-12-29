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

        // NOWE: Lista wszystkich nazw przedmiotów dla ComboBox
        public ObservableCollection<string> AllSubjectNames { get; set; } = new ObservableCollection<string>();

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
            UpdateAllSubjectNames(); // NOWE

            AddSubjectCommand = new RelayCommand(_ => AddSubject());
            RemoveSubjectCommand = new RelayCommand<SubjectModel>(RemoveSubject);
            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            SelectClassTypeCommand = new RelayCommand<string>(SelectClassType);

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
                UpdateAllSubjectNames(); // NOWE
            }
        }

        private void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(_allSubjects);
                File.WriteAllText("subjects.json", json);
                UpdateAllSubjectNames(); // NOWE - odśwież listę po zapisie
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

        // NOWA METODA: Aktualizuj listę wszystkich nazw przedmiotów
        private void UpdateAllSubjectNames()
        {
            AllSubjectNames.Clear();
            var uniqueNames = _allSubjects
                .Select(s => s.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .OrderBy(n => n);
            
            foreach (var name in uniqueNames)
            {
                AllSubjectNames.Add(name);
            }
            
            OnPropertyChanged(nameof(AllSubjectNames));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}