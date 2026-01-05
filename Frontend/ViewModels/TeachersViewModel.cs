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
            RemoveSubjectFromTeacherCommand = new RelayCommand<object>(RemoveSubjectFromTeacherWrapper);
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

        // ========================================
        // ✅ DODAWANIE Z WALIDACJĄ
        // ========================================
        private void AddSubjectToTeacher(TeacherModel teacher)
        {
            if (teacher == null) return;

            var selectWindow = new SubjectSelectionWindow(AllSubjects.ToList(), teacher.TaughtSubjects);
            if (selectWindow.ShowDialog() == true && selectWindow.SelectedSubject != null)
            {
                var selectedSubject = selectWindow.SelectedSubject;

                // ✅ WALIDACJA 1: Czy ten nauczyciel już ma dokładnie ten sam przedmiot?
                var duplicate = teacher.TaughtSubjects.FirstOrDefault(s => 
                    s.Name.Equals(selectedSubject.Name, StringComparison.OrdinalIgnoreCase) && 
                    s.Hours == selectedSubject.Hours &&
                    s.SubjectType.Equals(selectedSubject.SubjectType, StringComparison.OrdinalIgnoreCase) &&
                    s.ClassTypes.Count > 0 && selectedSubject.ClassTypes.Count > 0 &&
                    s.ClassTypes[0].Equals(selectedSubject.ClassTypes[0], StringComparison.OrdinalIgnoreCase));

                if (duplicate != null)
                {
                    MessageBox.Show(
                        $"⚠️ Nauczyciel '{teacher.Nazwisko}' już ma ten przedmiot:\n\n" +
                        $"📚 {selectedSubject.Name} ({selectedSubject.Hours}h)\n" +
                        $"Klasa: {selectedSubject.ClassTypes[0]}",
                        "Duplikat",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // ✅ WALIDACJA 2: Sprawdź czy INNY nauczyciel już uczy ten przedmiot w tej klasie
                var conflictingTeacher = FindConflictingTeacher(teacher, selectedSubject);
                
                if (conflictingTeacher != null)
                {
                    MessageBox.Show(
                        $"❌ KONFLIKT NAUCZYCIELA!\n\n" +
                        $"Przedmiot: {selectedSubject.Name} ({selectedSubject.Hours}h, {selectedSubject.SubjectType})\n" +
                        $"Klasa: {selectedSubject.ClassTypes[0]}\n\n" +
                        $"Jest już prowadzony przez:\n" +
                        $"👤 {conflictingTeacher.Nazwisko} (ID: {conflictingTeacher.Nr_pracownika})\n\n" +
                        $"❗ W jednej klasie przedmiot może prowadzić tylko JEDEN nauczyciel!\n\n" +
                        $"💡 Jeśli chcesz przypisać tego nauczyciela, najpierw usuń\n" +
                        $"   ten przedmiot u nauczyciela '{conflictingTeacher.Nazwisko}'.",
                        "Konflikt - Nie można dodać",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // Dodaj przedmiot
                teacher.TaughtSubjects.Add(selectedSubject);
                teacher.TaughtSubjects = new List<SubjectModel>(teacher.TaughtSubjects);
                OnPropertyChanged(nameof(Teachers));
                
                Console.WriteLine($"✅ Dodano: {teacher.Nazwisko} → {selectedSubject.Name} ({selectedSubject.ClassTypes[0]})");
            }
        }

        // ✅ Znajdź nauczyciela z konfliktem
        private TeacherModel FindConflictingTeacher(TeacherModel currentTeacher, SubjectModel newSubject)
        {
            // Jeśli przedmiot nie ma klasy - brak konfliktu
            if (newSubject.ClassTypes == null || !newSubject.ClassTypes.Any())
                return null;

            string newClassName = newSubject.ClassTypes[0];

            // Dla każdego innego nauczyciela
            foreach (var otherTeacher in Teachers)
            {
                // Pomiń obecnego nauczyciela
                if (otherTeacher.Nr_pracownika == currentTeacher.Nr_pracownika)
                    continue;

                // Sprawdź każdy przedmiot tego nauczyciela
                foreach (var existingSubject in otherTeacher.TaughtSubjects)
                {
                    // Jeśli przedmiot nie ma klasy - pomiń
                    if (existingSubject.ClassTypes == null || !existingSubject.ClassTypes.Any())
                        continue;

                    // Sprawdź czy to ten sam przedmiot (nazwa + godziny + typ)
                    bool isSameSubject = 
                        existingSubject.Name.Equals(newSubject.Name, StringComparison.OrdinalIgnoreCase) && 
                        existingSubject.Hours == newSubject.Hours &&
                        existingSubject.SubjectType.Equals(newSubject.SubjectType, StringComparison.OrdinalIgnoreCase);

                    // Sprawdź czy to ta sama klasa
                    bool isSameClass = existingSubject.ClassTypes[0].Equals(newClassName, StringComparison.OrdinalIgnoreCase);

                    if (isSameSubject && isSameClass)
                    {
                        // KONFLIKT! - ten nauczyciel już uczy tego przedmiotu w tej klasie
                        return otherTeacher;
                    }
                }
            }

            return null; // Brak konfliktu
        }

        private void RemoveSubjectFromTeacherWrapper(object parameter)
        {
            try
            {
                if (parameter is SubjectModel subject)
                {
                    var teacher = Teachers.FirstOrDefault(t => t.TaughtSubjects.Contains(subject));
                    if (teacher != null)
                    {
                        RemoveSubjectFromTeacher(teacher, subject);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd usuwania przedmiotu: {ex.Message}");
                MessageBox.Show($"❌ Błąd usuwania: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveSubjectFromTeacher(TeacherModel teacher, SubjectModel subject)
        {
            if (teacher != null && subject != null)
            {
                try
                {
                    teacher.TaughtSubjects.Remove(subject);
                    teacher.TaughtSubjects = new List<SubjectModel>(teacher.TaughtSubjects);
                    OnPropertyChanged(nameof(Teachers));
                    
                    Console.WriteLine($"✅ Usunięto: {subject.Name} od {teacher.Nazwisko}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd w RemoveSubjectFromTeacher: {ex.Message}");
                    throw;
                }
            }
        }

        private void Save()
        {
            try
            {
                // WALIDACJA 1: Duplikaty ID
                var duplicateIds = Teachers
                    .GroupBy(t => t.Nr_pracownika)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateIds.Any())
                {
                    MessageBox.Show(
                        $"❌ BŁĄD! Duplikaty ID:\n{string.Join(", ", duplicateIds)}",
                        "Błąd",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // WALIDACJA 2: Brak nazwisk
                var teachersWithoutName = Teachers
                    .Where(t => string.IsNullOrWhiteSpace(t.Nazwisko))
                    .Select(t => t.Nr_pracownika)
                    .ToList();

                if (teachersWithoutName.Any())
                {
                    MessageBox.Show(
                        $"❌ BŁĄD! Nauczyciele bez nazwiska:\nID: {string.Join(", ", teachersWithoutName)}",
                        "Błąd",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // ✅ WALIDACJA 3: Sprawdź konflikty przed zapisem
                var conflicts = FindAllConflicts();
                if (conflicts.Any())
                {
                    var summary = string.Join("\n", conflicts.Select(c => 
                        $"  • {c.SubjectName} ({c.ClassName}): {c.Teacher1} + {c.Teacher2}"));

                    MessageBox.Show(
                        $"❌ ZNALEZIONO {conflicts.Count} KONFLIKTÓW!\n\n" +
                        $"Następujące przedmioty w klasach mają 2+ nauczycieli:\n\n{summary}\n\n" +
                        $"Usuń duplikaty przed zapisem!",
                        "Błąd - Konflikty",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // Rozwiń dla backendu
                var expandedTeachers = ExpandTeachersForBackend();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(expandedTeachers, options);
                File.WriteAllText("teachers.json", json);

                MessageBox.Show(
                    $"✅ Zapisano!\n\nNauczyciele: {Teachers.Count}\nPrzypisania: {expandedTeachers.Count}",
                    "Sukces",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Błąd zapisu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ Znajdź wszystkie konflikty w systemie
        private List<ConflictInfo> FindAllConflicts()
        {
            var conflicts = new List<ConflictInfo>();
            var assignments = new Dictionary<string, List<(TeacherModel teacher, SubjectModel subject)>>();

            // Zgrupuj przypisania po (przedmiot + klasa)
            foreach (var teacher in Teachers)
            {
                foreach (var subject in teacher.TaughtSubjects)
                {
                    if (subject.ClassTypes == null || !subject.ClassTypes.Any())
                        continue;

                    string key = $"{subject.Name.ToLower()}_{subject.Hours}h_{subject.ClassTypes[0].ToLower()}";
                    
                    if (!assignments.ContainsKey(key))
                        assignments[key] = new List<(TeacherModel, SubjectModel)>();
                    
                    assignments[key].Add((teacher, subject));
                }
            }

            // Znajdź klucze gdzie jest więcej niż 1 nauczyciel
            foreach (var kvp in assignments.Where(x => x.Value.Count > 1))
            {
                var list = kvp.Value;
                
                conflicts.Add(new ConflictInfo
                {
                    SubjectName = list[0].subject.Name,
                    ClassName = list[0].subject.ClassTypes[0],
                    Teacher1 = list[0].teacher.Nazwisko,
                    Teacher2 = list[1].teacher.Nazwisko
                });
            }

            return conflicts;
        }

        private List<TeacherBackendModel> ExpandTeachersForBackend()
        {
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

        private bool CanSave() => true;

        private void LoadTeachers()
        {
            if (File.Exists("teachers.json"))
            {
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var json = File.ReadAllText("teachers.json");
                    var loaded = JsonSerializer.Deserialize<List<TeacherBackendModel>>(json, options);

                    if (loaded != null)
                    {
                        var grouped = loaded
                            .GroupBy(t => t.Nr_pracownika)
                            .Select(g => new TeacherModel
                            {
                                Nr_pracownika = g.Key,
                                Nazwisko = g.First().Nazwisko,
                                Availability = g.First().Availability ?? new List<List<bool>>(),
                                TaughtSubjects = g.Select(t => t.SelectedSubject).Where(s => s != null).ToList()
                            });

                        Teachers = new ObservableCollection<TeacherModel>(grouped);
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

        private class ConflictInfo
        {
            public string SubjectName { get; set; }
            public string ClassName { get; set; }
            public string Teacher1 { get; set; }
            public string Teacher2 { get; set; }
        }
    }

    public class TeacherBackendModel
    {
        public int Nr_pracownika { get; set; }
        public string Nazwisko { get; set; }
        public List<List<bool>> Availability { get; set; }
        public SubjectModel SelectedSubject { get; set; }
    }
}