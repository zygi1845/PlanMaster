using Frontend.Models;
using Frontend.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Frontend.ViewModels
{
    public class TeachersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<TeacherModel> _teachers = new ObservableCollection<TeacherModel>();
        private ObservableCollection<SubjectModel> _allSubjects = new ObservableCollection<SubjectModel>();

        public ObservableCollection<TeacherModel> Teachers
        {
            get => _teachers;
            set
            {
                _teachers = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<SubjectModel> AllSubjects
        {
            get => _allSubjects;
            set
            {
                _allSubjects = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddTeacherCommand { get; }
        public ICommand RemoveTeacherCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand AddSubjectToTeacherCommand { get; }
        public ICommand RemoveSubjectFromTeacherCommand { get; }

        public TeachersViewModel()
        {
            LoadSubjects();
            LoadTeachers();

            AddTeacherCommand = new RelayCommand(_ => AddTeacher());
            RemoveTeacherCommand = new RelayCommand<TeacherModel>(RemoveTeacher);
            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            AddSubjectToTeacherCommand = new RelayCommand<TeacherModel>(AddSubjectToTeacher);
            RemoveSubjectFromTeacherCommand = new RelayCommand<(TeacherModel, SubjectModel)>(tuple => 
                RemoveSubjectFromTeacher(tuple.Item1, tuple.Item2));
        }

        private void LoadSubjects()
        {
            if (File.Exists("subjects.json"))
            {
                try
                {
                    var json = File.ReadAllText("subjects.json");
                    var loaded = JsonSerializer.Deserialize<ObservableCollection<SubjectModel>>(json);
                    AllSubjects = loaded ?? new ObservableCollection<SubjectModel>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd wczytywania przedmiotów: {ex.Message}");
                    AllSubjects = new ObservableCollection<SubjectModel>();
                }
            }
        }

        private void AddTeacher()
        {
            Teachers.Add(new TeacherModel
            {
                Nr_pracownika = GetNextTeacherId(),
                Nazwisko = "",
                Availability = new List<List<bool>>(),
                TaughtSubjects = new List<SubjectModel>()
            });
        }

        private int GetNextTeacherId()
        {
            return Teachers.Any() ? Teachers.Max(t => t.Nr_pracownika) + 1 : 1;
        }

        private void RemoveTeacher(TeacherModel teacher)
        {
            if (teacher != null)
            {
                var result = MessageBox.Show(
                    $"❓ Czy na pewno usunąć nauczyciela:\n\n{teacher.Nazwisko} (Nr: {teacher.Nr_pracownika})?",
                    "Potwierdzenie usunięcia",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Teachers.Remove(teacher);
                }
            }
        }

        private void AddSubjectToTeacher(TeacherModel teacher)
        {
            if (teacher == null) return;

            // Pokaż okno wyboru przedmiotu
            var selectWindow = new SubjectSelectionWindow(AllSubjects.ToList(), teacher.TaughtSubjects);
            if (selectWindow.ShowDialog() == true && selectWindow.SelectedSubject != null)
            {
                // Sprawdź czy już nie ma tego przedmiotu
                if (!teacher.TaughtSubjects.Any(s => s.Name == selectWindow.SelectedSubject.Name))
                {
                    teacher.TaughtSubjects.Add(selectWindow.SelectedSubject);
                    teacher.TaughtSubjects = new List<SubjectModel>(teacher.TaughtSubjects); // Trigger PropertyChanged
                }
                else
                {
                    MessageBox.Show(
                        $"⚠️ Nauczyciel już uczy przedmiotu: {selectWindow.SelectedSubject.Name}",
                        "Duplikat",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
        }

        private void RemoveSubjectFromTeacher(TeacherModel teacher, SubjectModel subject)
        {
            if (teacher != null && subject != null)
            {
                teacher.TaughtSubjects.Remove(subject);
                teacher.TaughtSubjects = new List<SubjectModel>(teacher.TaughtSubjects); // Trigger PropertyChanged
            }
        }

        private void Save()
        {
            try
            {
                // WALIDACJA 1: Sprawdź duplikaty Nr_pracownika
                var duplicateIds = Teachers
                    .GroupBy(t => t.Nr_pracownika)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateIds.Any())
                {
                    MessageBox.Show(
                        $"❌ BŁĄD DUPLIKATÓW!\n\n" +
                        $"Numery pracowników występują więcej niż raz:\n" +
                        $"{string.Join(", ", duplicateIds)}\n\n" +
                        $"Każdy nauczyciel musi mieć UNIKALNY numer!",
                        "Błąd walidacji",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // WALIDACJA 2: Sprawdź czy wszyscy mają nazwisko
                var teachersWithoutName = Teachers
                    .Where(t => string.IsNullOrWhiteSpace(t.Nazwisko))
                    .Select(t => t.Nr_pracownika)
                    .ToList();

                if (teachersWithoutName.Any())
                {
                    MessageBox.Show(
                        $"❌ BŁĄD!\n\n" +
                        $"Następujący nauczyciele nie mają nazwiska:\n" +
                        $"Nr: {string.Join(", ", teachersWithoutName)}",
                        "Błąd walidacji",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // WALIDACJA 3: Sprawdź czy wszyscy mają przynajmniej jeden przedmiot
                var teachersWithoutSubjects = Teachers
                    .Where(t => !t.TaughtSubjects.Any())
                    .Select(t => t.Nazwisko)
                    .ToList();

                if (teachersWithoutSubjects.Any())
                {
                    var result = MessageBox.Show(
                        $"⚠️ OSTRZEŻENIE!\n\n" +
                        $"Następujący nauczyciele nie mają przypisanych przedmiotów:\n" +
                        $"{string.Join(", ", teachersWithoutSubjects)}\n\n" +
                        $"Czy na pewno chcesz zapisać?",
                        "Brak przedmiotów",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                        return;
                }

                // Konwertuj na format dla backendu (rozwiń na wiele wpisów)
                var expandedTeachers = ExpandTeachersForBackend();

                // ZAPIS CZYSTEGO JSON (bez $id, $values)
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                };

                var json = JsonSerializer.Serialize(expandedTeachers, options);
                File.WriteAllText("teachers.json", json);

                MessageBox.Show(
                    $"✅ Zapisano pomyślnie!\n\n" +
                    $"Liczba nauczycieli: {Teachers.Count}\n" +
                    $"Łączna liczba przypisań przedmiot-nauczyciel: {expandedTeachers.Count}",
                    "Sukces",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    $"❌ Błąd zapisu do pliku:\n\n{ex.Message}",
                    "Błąd I/O",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Nieoczekiwany błąd:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Błąd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private List<TeacherBackendModel> ExpandTeachersForBackend()
        {
            // Każdy nauczyciel + przedmiot = osobny wpis dla backendu
            var result = new List<TeacherBackendModel>();

            foreach (var teacher in Teachers)
            {
                foreach (var subject in teacher.TaughtSubjects)
                {
                    result.Add(new TeacherBackendModel
                    {
                        Nr_pracownika = teacher.Nr_pracownika,
                        Nazwisko = teacher.Nazwisko,
                        Availability = teacher.Availability,
                        SelectedSubject = subject
                    });
                }
            }

            return result;
        }

        private bool CanSave()
        {
            return true;
        }

        private void LoadTeachers()
        {
            if (File.Exists("teachers.json"))
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    };

                    var json = File.ReadAllText("teachers.json");
                    var loadedBackendFormat = JsonSerializer.Deserialize<List<TeacherBackendModel>>(json, options);

                    if (loadedBackendFormat != null)
                    {
                        // Zgrupuj po Nr_pracownika (ten sam nauczyciel może mieć wiele przedmiotów)
                        var groupedTeachers = loadedBackendFormat
                            .GroupBy(t => t.Nr_pracownika)
                            .Select(g => new TeacherModel
                            {
                                Nr_pracownika = g.Key,
                                Nazwisko = g.First().Nazwisko,
                                Availability = g.First().Availability ?? new List<List<bool>>(),
                                TaughtSubjects = g.Select(t => t.SelectedSubject)
                                                  .Where(s => s != null)
                                                  .ToList()
                            });

                        Teachers = new ObservableCollection<TeacherModel>(groupedTeachers);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd wczytywania: {ex.Message}");
                    Teachers = new ObservableCollection<TeacherModel>();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Pomocnicza klasa dla formatu backendu
    public class TeacherBackendModel
    {
        public int Nr_pracownika { get; set; }
        public string Nazwisko { get; set; }
        public List<List<bool>> Availability { get; set; }
        public SubjectModel SelectedSubject { get; set; }
    }
}