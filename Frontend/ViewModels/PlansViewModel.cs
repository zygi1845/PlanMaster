using Frontend.Views;
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
    public enum ViewType
    {
        Classes,
        Teachers,
        Rooms,
        Errors
    }

    public class PlansViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<PlanItem> _items = new ObservableCollection<PlanItem>();
        private ViewType _selectedViewType = ViewType.Classes;

        // Przechowuj surowe dane z Pythona
        private Dictionary<string, List<List<List<JsonElement>>>> _rawClassData;
        private Dictionary<string, List<List<List<JsonElement>>>> _rawTeacherData;
        private Dictionary<string, List<List<List<JsonElement>>>> _rawRoomData;
        private List<string> _hourPlan;
        
        private readonly string[] _daysOfWeek = { "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek" };
        private const int WORKING_DAYS = 5; // Pn-Pt

        public ObservableCollection<PlanItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        public ViewType SelectedViewType
        {
            get => _selectedViewType;
            set
            {
                _selectedViewType = value;
                OnPropertyChanged();
                OnSelectedViewTypeChanged();
            }
        }

        public ICommand ShowDetailsCommand { get; }

        public PlansViewModel()
        {
            LoadHourPlan();
            LoadData();
            ShowDetailsCommand = new RelayCommand<PlanItem>(ShowDetails);
        }

        private void OnSelectedViewTypeChanged()
        {
            Items.Clear();
            
            switch (SelectedViewType)
            {
                case ViewType.Classes:
                    LoadClasses();
                    break;
                case ViewType.Teachers:
                    LoadTeachers();
                    break;
                case ViewType.Rooms:
                    LoadRooms();
                    break;
                case ViewType.Errors:
                    LoadErrors();
                    break;
            }
        }

        private string GetPlansBasePath()
        {
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo dir = new DirectoryInfo(currentPath);
            
            while (dir != null && dir.Name != "Inzynierka")
            {
                dir = dir.Parent;
            }
            
            if (dir != null)
            {
                string plansPath = Path.Combine(dir.FullName, "Backend", "algotithm", "plany");
                Console.WriteLine($"Ścieżka do planów: {plansPath}");
                return plansPath;
            }
            
            Console.WriteLine("BŁĄD: Nie znaleziono katalogu Inzynierka!");
            return null;
        }

        private void LoadHourPlan()
        {
            try
            {
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo dir = new DirectoryInfo(currentPath);
                
                while (dir != null && dir.Name != "Inzynierka")
                {
                    dir = dir.Parent;
                }
                
                if (dir != null)
                {
                    string hourPlanPath = Path.Combine(dir.FullName, "hourplan.json");
                    if (File.Exists(hourPlanPath))
                    {
                        var json = File.ReadAllText(hourPlanPath);
                        using (JsonDocument doc = JsonDocument.Parse(json))
                        {
                            _hourPlan = new List<string>();
                            foreach (var item in doc.RootElement.EnumerateArray())
                            {
                                if (item.TryGetProperty("Hour", out JsonElement hourElement))
                                {
                                    _hourPlan.Add(hourElement.GetString());
                                }
                            }
                            Console.WriteLine($"Wczytano {_hourPlan.Count} godzin lekcyjnych");
                        }
                    }
                }
                
                if (_hourPlan == null || _hourPlan.Count == 0)
                {
                    _hourPlan = new List<string> { "8:00-8:55", "9:00-9:55", "10:00-10:55", "11:00-11:55", "12:00-12:55", "13:00-13:55" };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd ładowania planu godzinowego: {ex.Message}");
                _hourPlan = new List<string> { "8:00-8:55", "9:00-9:55", "10:00-10:55", "11:00-11:55", "12:00-12:55", "13:00-13:55" };
            }
        }

        private void LoadData()
        {
            Console.WriteLine("Ładowanie danych...");
            LoadClasses();
            Console.WriteLine($"Załadowano dane");
        }

        private void LoadClasses()
        {
            try
            {
                string basePath = GetPlansBasePath();
                if (basePath == null) return;
                
                string filePath = Path.Combine(basePath, "class_schedules.json");
                
                Console.WriteLine($"Szukam pliku: {filePath}");
                
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    Console.WriteLine($"Znaleziono plik, rozmiar: {json.Length}");
                    
                    _rawClassData = JsonSerializer.Deserialize<Dictionary<string, List<List<List<JsonElement>>>>>(json);

                    Items.Clear();
                    foreach (var className in _rawClassData.Keys)
                    {
                        Items.Add(new PlanItem { Name = className, Type = "Class", Icon = "" });
                        Console.WriteLine($"Dodano klasę: {className}");
                    }
                }
                else
                {
                    Console.WriteLine($"Plik nie istnieje: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BŁĄD wczytywania klas: {ex.Message}");
                MessageBox.Show($"Błąd wczytywania klas: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTeachers()
        {
            try
            {
                string basePath = GetPlansBasePath();
                if (basePath == null) return;
                
                string filePath = Path.Combine(basePath, "teacher_schedules.json");
                
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    _rawTeacherData = JsonSerializer.Deserialize<Dictionary<string, List<List<List<JsonElement>>>>>(json);

                    Items.Clear();
                    foreach (var teacherName in _rawTeacherData.Keys)
                    {
                        Items.Add(new PlanItem { Name = teacherName, Type = "Teacher", Icon = "" });
                        Console.WriteLine($"Dodano nauczyciela: {teacherName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BŁĄD wczytywania nauczycieli: {ex.Message}");
            }
        }

        private void LoadRooms()
        {
            try
            {
                string basePath = GetPlansBasePath();
                if (basePath == null) return;
                
                string filePath = Path.Combine(basePath, "room_schedules.json");
                
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    _rawRoomData = JsonSerializer.Deserialize<Dictionary<string, List<List<List<JsonElement>>>>>(json);

                    Items.Clear();
                    foreach (var roomName in _rawRoomData.Keys)
                    {
                        Items.Add(new PlanItem { Name = roomName, Type = "Room", Icon = "" });
                        Console.WriteLine($"Dodano salę: {roomName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BŁĄD wczytywania sal: {ex.Message}");
            }
        }

        private void ReloadAllData()
        {
            try
            {
                Console.WriteLine("Przeładowuję dane z dysku...");
                
                string basePath = GetPlansBasePath();
                if (basePath == null) return;

                string classFilePath = Path.Combine(basePath, "class_schedules.json");
                if (File.Exists(classFilePath))
                {
                    var json = File.ReadAllText(classFilePath);
                    _rawClassData = JsonSerializer.Deserialize<Dictionary<string, List<List<List<JsonElement>>>>>(json);
                    Console.WriteLine("Przeładowano dane klas");
                }

                string teacherFilePath = Path.Combine(basePath, "teacher_schedules.json");
                if (File.Exists(teacherFilePath))
                {
                    var json = File.ReadAllText(teacherFilePath);
                    _rawTeacherData = JsonSerializer.Deserialize<Dictionary<string, List<List<List<JsonElement>>>>>(json);
                    Console.WriteLine("Przeładowano dane nauczycieli");
                }

                string roomFilePath = Path.Combine(basePath, "room_schedules.json");
                if (File.Exists(roomFilePath))
                {
                    var json = File.ReadAllText(roomFilePath);
                    _rawRoomData = JsonSerializer.Deserialize<Dictionary<string, List<List<List<JsonElement>>>>>(json);
                    Console.WriteLine("Przeładowano dane sal");
                }

                Console.WriteLine("Wszystkie dane przeładowane");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd przeładowania danych: {ex.Message}");
            }
        }

        private List<string> AnalyzeAllConflicts()
        {
            var allErrors = new List<string>();

            try
            {
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo dir = new DirectoryInfo(currentPath);

                while (dir != null && dir.Name != "Inzynierka")
                {
                    dir = dir.Parent;
                }

                if (dir == null) return allErrors;

                string plansPath = Path.Combine(dir.FullName, "Backend", "algotithm", "plany");
                string classFilePath = Path.Combine(plansPath, "class_schedules.json");

                if (!File.Exists(classFilePath))
                    return allErrors;

                var jsonString = File.ReadAllText(classFilePath);
                using (JsonDocument document = JsonDocument.Parse(jsonString))
                {
                    // 1. KONFLIKTY NAUCZYCIELI, SAL I KLAS
                    for (int dayIndex = 0; dayIndex < WORKING_DAYS; dayIndex++)
                    {
                        string dayName = _daysOfWeek[dayIndex];

                        for (int hourIndex = 0; hourIndex < _hourPlan.Count; hourIndex++)
                        {
                            string hourTime = _hourPlan[hourIndex];

                            var lessonsAtThisTime = new List<(string className, string teacher, string room, string subject)>();

                            foreach (JsonProperty planProperty in document.RootElement.EnumerateObject())
                            {
                                string className = planProperty.Name;

                                if (planProperty.Value.ValueKind != JsonValueKind.Array)
                                    continue;

                                var daysArray = planProperty.Value;

                                if (dayIndex >= daysArray.GetArrayLength())
                                    continue;

                                var daySchedule = daysArray[dayIndex];

                                if (hourIndex >= daySchedule.GetArrayLength())
                                    continue;

                                var hourLessons = daySchedule[hourIndex];

                                foreach (JsonElement lessonElement in hourLessons.EnumerateArray())
                                {
                                    string teacher = GetJsonStringProperty(lessonElement, "teacher");
                                    string room = GetJsonStringProperty(lessonElement, "room");
                                    string subject = GetJsonStringProperty(lessonElement, "subject");

                                    lessonsAtThisTime.Add((className, teacher, room, subject));
                                }
                            }

                            if (lessonsAtThisTime.Count > 1)
                            {
                                // Konflikty nauczycieli
                                var teacherGroups = lessonsAtThisTime.GroupBy(l => l.teacher).Where(g => g.Count() > 1);
                                foreach (var group in teacherGroups)
                                {
                                    var lessons = group.ToList();
                                    string classes = string.Join(", ", lessons.Select(l => l.className));
                                    allErrors.Add($"Nauczyciel '{group.Key}' ma zajęcia w {lessons.Count} klasach: {classes} ({dayName} {hourTime})");
                                }

                                // Konflikty sal
                                var roomGroups = lessonsAtThisTime.GroupBy(l => l.room).Where(g => g.Count() > 1);
                                foreach (var group in roomGroups)
                                {
                                    var lessons = group.ToList();
                                    string classes = string.Join(", ", lessons.Select(l => l.className));
                                    allErrors.Add($"Sala '{group.Key}' zajęta przez {lessons.Count} klasy: {classes} ({dayName} {hourTime})");
                                }
                                
                                // ✅ NOWE: Konflikty klas (ta sama klasa ma 2+ lekcje jednocześnie)
                                var classGroups = lessonsAtThisTime.GroupBy(l => l.className).Where(g => g.Count() > 1);
                                foreach (var group in classGroups)
                                {
                                    var lessons = group.ToList();
                                    string subjects = string.Join(", ", lessons.Select(l => l.subject));
                                    allErrors.Add($"Klasa '{group.Key}' ma {lessons.Count} lekcje jednocześnie: {subjects} ({dayName} {hourTime})");
                                }
                            }
                        }
                    }

                    // 2. LUKI/DZIURY W PLANACH KLAS
                    // 3. LIMIT GODZIN DZIENNIE (OGÓLNY)
                    // 4. SUMA GODZIN W TYGODNIU
                    foreach (JsonProperty planProperty in document.RootElement.EnumerateObject())
                    {
                        string className = planProperty.Name;

                        if (planProperty.Value.ValueKind != JsonValueKind.Array)
                            continue;

                        var daysArray = planProperty.Value;

                        int totalWeeklyHours = 0;

                        int daysToCheck = Math.Min(WORKING_DAYS, daysArray.GetArrayLength());
                        
                        for (int dayIndex = 0; dayIndex < daysToCheck; dayIndex++)
                        {
                            string dayName = _daysOfWeek[dayIndex];
                            var daySchedule = daysArray[dayIndex];

                            // Luki
                            var gaps = FindGapsInDay(daySchedule, className, dayName);
                            allErrors.AddRange(gaps);

                            // Liczba godzin w dniu (OGÓLNIE)
                            int dailyHours = CountHoursInDay(daySchedule);
                            totalWeeklyHours += dailyHours;

                            // Sprawdź limit dzienny (OGÓLNY)
                            int maxHoursPerDay = GetMaxHoursPerDay();
                            if (dailyHours > maxHoursPerDay)
                            {
                                allErrors.Add($"Przekroczono limit godzin: Klasa '{className}' ma {dailyHours} godzin w {dayName} (max: {maxHoursPerDay})");
                            }

                            // Sprawdź limit godzin KAŻDEGO PRZEDMIOTU dziennie
                            var subjectHoursExceeded = CheckSubjectDailyLimit(daySchedule, className, dayName);
                            allErrors.AddRange(subjectHoursExceeded);
                        }

                        // Sprawdź sumę tygodniową
                        int expectedWeeklyHours = GetExpectedWeeklyHours(className);
                        
                        Console.WriteLine($"Klasa '{className}': ma {totalWeeklyHours}h, oczekiwano {expectedWeeklyHours}h");
                        
                        if (expectedWeeklyHours > 0 && totalWeeklyHours != expectedWeeklyHours)
                        {
                            string diff = totalWeeklyHours > expectedWeeklyHours ?
                                $"+{totalWeeklyHours - expectedWeeklyHours}" :
                                $"{totalWeeklyHours - expectedWeeklyHours}";

                            allErrors.Add($"Nieprawidłowa liczba godzin: Klasa '{className}' ma {totalWeeklyHours} godzin (oczekiwano: {expectedWeeklyHours}, różnica: {diff})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd analizy: {ex.Message}");
            }

            return allErrors;
        }

        private List<string> CheckSubjectDailyLimit(JsonElement daySchedule, string className, string dayName)
        {
            var errors = new List<string>();

            try
            {
                // Policz ile godzin każdego przedmiotu jest w tym dniu
                var subjectHoursCount = new Dictionary<string, int>();

                for (int hourIndex = 0; hourIndex < daySchedule.GetArrayLength(); hourIndex++)
                {
                    var hourLessons = daySchedule[hourIndex];

                    foreach (JsonElement lessonElement in hourLessons.EnumerateArray())
                    {
                        string subject = GetJsonStringProperty(lessonElement, "subject");
                        
                        if (!string.IsNullOrEmpty(subject))
                        {
                            if (!subjectHoursCount.ContainsKey(subject))
                                subjectHoursCount[subject] = 0;
                            
                            subjectHoursCount[subject]++;
                        }
                    }
                }

                // Sprawdź limity z weights_config.json
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo dir = new DirectoryInfo(currentPath);

                while (dir != null && dir.Name != "Inzynierka")
                {
                    dir = dir.Parent;
                }

                if (dir != null)
                {
                    string weightsPath = Path.Combine(dir.FullName, "weights_config.json");

                    if (File.Exists(weightsPath))
                    {
                        var json = File.ReadAllText(weightsPath);
                        using (JsonDocument doc = JsonDocument.Parse(json))
                        {
                            if (doc.RootElement.TryGetProperty("SubjectWeights", out JsonElement subjectWeights))
                            {
                                foreach (JsonElement subjectWeight in subjectWeights.EnumerateArray())
                                {
                                    string subjectName = subjectWeight.TryGetProperty("Name", out JsonElement name) 
                                        ? name.GetString() : "";
                                    
                                    int maxPerDay = subjectWeight.TryGetProperty("MaxPerDay", out JsonElement maxPD) 
                                        ? maxPD.GetInt32() : 999;

                                    // Sprawdź czy ten przedmiot przekroczył limit
                                    if (subjectHoursCount.ContainsKey(subjectName))
                                    {
                                        int actualHours = subjectHoursCount[subjectName];
                                        
                                        if (actualHours > maxPerDay)
                                        {
                                            errors.Add($"Przekroczono limit przedmiotu: Klasa '{className}' ma {actualHours} godzin przedmiotu '{subjectName}' w {dayName} (max: {maxPerDay})");
                                            Console.WriteLine($"PRZEKROCZENIE: {className} - {subjectName}: {actualHours}h (max {maxPerDay}h) w {dayName}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd CheckSubjectDailyLimit: {ex.Message}");
            }

            return errors;
        }

        private int CountHoursInDay(JsonElement daySchedule)
        {
            int count = 0;

            for (int hourIndex = 0; hourIndex < daySchedule.GetArrayLength(); hourIndex++)
            {
                var hourLessons = daySchedule[hourIndex];
                if (hourLessons.GetArrayLength() > 0)
                {
                    count++;
                }
            }

            return count;
        }

        private int GetMaxHoursPerDay()
        {
            try
            {
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo dir = new DirectoryInfo(currentPath);

                while (dir != null && dir.Name != "Inzynierka")
                {
                    dir = dir.Parent;
                }

                if (dir != null)
                {
                    string weightsPath = Path.Combine(dir.FullName, "weights_config.json");

                    if (File.Exists(weightsPath))
                    {
                        var json = File.ReadAllText(weightsPath);
                        using (JsonDocument doc = JsonDocument.Parse(json))
                        {
                            if (doc.RootElement.TryGetProperty("MaxHoursPerDay", out JsonElement maxHours))
                            {
                                return maxHours.GetInt32();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd wczytywania MaxHoursPerDay: {ex.Message}");
            }

            return 8;
        }

        private int GetExpectedWeeklyHours(string className)
        {
            try
            {
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo dir = new DirectoryInfo(currentPath);

                while (dir != null && dir.Name != "Inzynierka")
                {
                    dir = dir.Parent;
                }

                if (dir == null) return 0;

                string subjectsPath = Path.Combine(dir.FullName, "subjects.json");

                if (!File.Exists(subjectsPath))
                    return 0;

                var json = File.ReadAllText(subjectsPath);
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    int totalHours = 0;

                    string baseClassName = className;
                    int lastSpace = className.LastIndexOf(' ');
                    if (lastSpace > 0 && int.TryParse(className.Substring(lastSpace + 1), out _))
                    {
                        baseClassName = className.Substring(0, lastSpace).Trim();
                    }

                    Console.WriteLine($"Sprawdzam przedmioty dla klasy '{className}' (typ: '{baseClassName}')");

                    foreach (JsonElement subjectElement in doc.RootElement.EnumerateArray())
                    {
                        if (subjectElement.TryGetProperty("ClassTypes", out JsonElement classTypes))
                        {
                            bool isForThisClass = false;

                            foreach (JsonElement classType in classTypes.EnumerateArray())
                            {
                                string classTypeName = classType.GetString();

                                if (classTypeName == baseClassName)
                                {
                                    isForThisClass = true;
                                    break;
                                }
                            }

                            if (isForThisClass)
                            {
                                if (subjectElement.TryGetProperty("Hours", out JsonElement hours))
                                {
                                    int subjectHours = hours.GetInt32();
                                    string subjectName = subjectElement.TryGetProperty("Name", out JsonElement name) 
                                        ? name.GetString() : "?";
                                    
                                    totalHours += subjectHours;
                                    Console.WriteLine($"  + {subjectName}: {subjectHours}h");
                                }
                            }
                        }
                    }

                    Console.WriteLine($"SUMA dla '{className}': {totalHours}h");
                    return totalHours;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd wczytywania expected hours: {ex.Message}");
                return 0;
            }
        }

        private List<string> FindGapsInDay(JsonElement daySchedule, string className, string dayName)
        {
            var gaps = new List<string>();

            try
            {
                var occupiedHours = new List<int>();

                for (int hourIndex = 0; hourIndex < Math.Min(daySchedule.GetArrayLength(), _hourPlan.Count); hourIndex++)
                {
                    var hourLessons = daySchedule[hourIndex];

                    if (hourLessons.GetArrayLength() > 0)
                    {
                        occupiedHours.Add(hourIndex);
                    }
                }

                if (occupiedHours.Count == 0)
                    return gaps;

                int firstLesson = occupiedHours.Min();
                int lastLesson = occupiedHours.Max();

                for (int hour = firstLesson + 1; hour < lastLesson; hour++)
                {
                    if (!occupiedHours.Contains(hour))
                    {
                        string gapHour = _hourPlan[hour];
                        gaps.Add($"Luka w planie: Klasa '{className}' ma przerwę {gapHour} ({dayName})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd wykrywania luk: {ex.Message}");
            }

            return gaps;
        }

        private void LoadErrors()
        {
            try
            {
                Console.WriteLine("Ładowanie błędów...");

                var errors = AnalyzeAllConflicts();

                Items.Clear();

                Items.Add(new PlanItem
                {
                    Name = $"═══════════════════════════════════",
                    Type = "Separator",
                    Icon = ""
                });

                Items.Add(new PlanItem
                {
                    Name = $"PODSUMOWANIE BŁĘDÓW",
                    Type = "Header",
                    Icon = ""
                });

                int teacherConflicts = errors.Count(e => e.Contains("Nauczyciel"));
                int roomConflicts = errors.Count(e => e.Contains("Sala"));
                int classConflicts = errors.Count(e => e.Contains("ma") && e.Contains("lekcje jednocześnie")); // ✅ NOWE
                int gaps = errors.Count(e => e.Contains("Luka"));
                int dailyLimit = errors.Count(e => e.Contains("Przekroczono limit godzin"));
                int subjectLimit = errors.Count(e => e.Contains("Przekroczono limit przedmiotu"));
                int weeklyHours = errors.Count(e => e.Contains("Nieprawidłowa"));

                Items.Add(new PlanItem
                {
                    Name = $"Konfliktów nauczycieli: {teacherConflicts}",
                    Type = "Summary",
                    Icon = ""
                });

                Items.Add(new PlanItem
                {
                    Name = $"Konfliktów sal: {roomConflicts}",
                    Type = "Summary",
                    Icon = ""
                });

                // ✅ NOWE: Wyświetl konflikty klas
                Items.Add(new PlanItem
                {
                    Name = $"Konfliktów klas: {classConflicts}",
                    Type = "Summary",
                    Icon = ""
                });

                Items.Add(new PlanItem
                {
                    Name = $"Luk w planach: {gaps}",
                    Type = "Summary",
                    Icon = ""
                });

                Items.Add(new PlanItem
                {
                    Name = $"Przekroczeń limitu dziennego: {dailyLimit}",
                    Type = "Summary",
                    Icon = ""
                });

                Items.Add(new PlanItem
                {
                    Name = $"Przekroczeń limitu przedmiotu: {subjectLimit}",
                    Type = "Summary",
                    Icon = ""
                });

                Items.Add(new PlanItem
                {
                    Name = $"Błędów sumy tygodniowej: {weeklyHours}",
                    Type = "Summary",
                    Icon = ""
                });

                Items.Add(new PlanItem
                {
                    Name = $"Łącznie problemów: {errors.Count}",
                    Type = "Summary",
                    Icon = ""
                });

                int penaltyPoints = CalculatePenaltyPoints(errors);

                Items.Add(new PlanItem
                {
                    Name = $"Punkty karne: {penaltyPoints}",
                    Type = "Summary",
                    Icon = ""
                });

                Items.Add(new PlanItem
                {
                    Name = $"═══════════════════════════════════",
                    Type = "Separator",
                    Icon = ""
                });

                if (errors.Count == 0)
                {
                    Items.Add(new PlanItem
                    {
                        Name = "Brak błędów! Plan jest idealny!",
                        Type = "Success",
                        Icon = ""
                    });
                }
                else
                {
                    if (teacherConflicts > 0)
                    {
                        Items.Add(new PlanItem { Name = $"KONFLIKTY NAUCZYCIELI ({teacherConflicts}):", Type = "Header", Icon = "" });
                        AddErrorsToList(errors, "Nauczyciel");
                    }

                    if (roomConflicts > 0)
                    {
                        Items.Add(new PlanItem { Name = $"KONFLIKTY SAL ({roomConflicts}):", Type = "Header", Icon = "" });
                        AddErrorsToList(errors, "Sala");
                    }

                    // ✅ NOWE: Sekcja z konfliktami klas
                    if (classConflicts > 0)
                    {
                        Items.Add(new PlanItem { Name = $"KONFLIKTY KLAS ({classConflicts}):", Type = "Header", Icon = "" });
                        AddErrorsToList(errors, "lekcje jednocześnie");
                    }

                    if (gaps > 0)
                    {
                        Items.Add(new PlanItem { Name = $"LUKI W PLANACH ({gaps}):", Type = "Header", Icon = "" });
                        AddErrorsToList(errors, "Luka");
                    }

                    if (dailyLimit > 0)
                    {
                        Items.Add(new PlanItem { Name = $"PRZEKROCZENIA LIMITU DZIENNEGO ({dailyLimit}):", Type = "Header", Icon = "" });
                        AddErrorsToList(errors, "Przekroczono limit godzin");
                    }

                    if (subjectLimit > 0)
                    {
                        Items.Add(new PlanItem { Name = $"PRZEKROCZENIA LIMITU PRZEDMIOTU ({subjectLimit}):", Type = "Header", Icon = "" });
                        AddErrorsToList(errors, "Przekroczono limit przedmiotu");
                    }

                    if (weeklyHours > 0)
                    {
                        Items.Add(new PlanItem { Name = $"BŁĘDY SUMY GODZIN ({weeklyHours}):", Type = "Header", Icon = "" });
                        AddErrorsToList(errors, "Nieprawidłowa");
                    }

                    Items.Add(new PlanItem
                    {
                        Name = $"═══════════════════════════════════",
                        Type = "Separator",
                        Icon = ""
                    });
                }

                Console.WriteLine($"Załadowano {errors.Count} błędów, {penaltyPoints} punktów karnych");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd ładowania błędów: {ex.Message}");
                MessageBox.Show($"Błąd ładowania błędów: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddErrorsToList(List<string> allErrors, string filterText)
        {
            int num = 1;
            foreach (var error in allErrors.Where(e => e.Contains(filterText)))
            {
                Items.Add(new PlanItem
                {
                    Name = $"{num}. {error}",
                    Type = "Error",
                    Icon = ""
                });
                num++;
            }

            Items.Add(new PlanItem
            {
                Name = $"───────────────────────────────────",
                Type = "Separator",
                Icon = ""
            });
        }

        private int CalculatePenaltyPoints(List<string> errors)
        {
            int points = 0;

            try
            {
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo dir = new DirectoryInfo(currentPath);

                while (dir != null && dir.Name != "Inzynierka")
                {
                    dir = dir.Parent;
                }

                if (dir == null)
                {
                    Console.WriteLine("Nie znaleziono katalogu Inzynierka - używam domyślnych wag");
                    return CalculateDefaultPenalties(errors);
                }

                string weightsPath = Path.Combine(dir.FullName, "weights_config.json");

                if (!File.Exists(weightsPath))
                {
                    Console.WriteLine("Brak weights_config.json - używam domyślnych wag");
                    return CalculateDefaultPenalties(errors);
                }

                var json = File.ReadAllText(weightsPath);
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    int maxHoursWeight = doc.RootElement.TryGetProperty("MaxHoursWeight", out JsonElement mhw) 
                        ? mhw.GetInt32() : 5;
                    
                    int breaksWeight = doc.RootElement.TryGetProperty("BreaksWeight", out JsonElement bw) 
                        ? bw.GetInt32() : 3;

                    var subjectWeights = new Dictionary<string, int>();
                    
                    if (doc.RootElement.TryGetProperty("SubjectWeights", out JsonElement subjectWeightsArray))
                    {
                        foreach (JsonElement sw in subjectWeightsArray.EnumerateArray())
                        {
                            string name = sw.TryGetProperty("Name", out JsonElement nameEl) 
                                ? nameEl.GetString() : "";
                            
                            int weight = sw.TryGetProperty("Weight", out JsonElement weightEl) 
                                ? weightEl.GetInt32() : 5;

                            if (!string.IsNullOrEmpty(name))
                            {
                                subjectWeights[name] = weight;
                                Console.WriteLine($"Waga dla '{name}': {weight}");
                            }
                        }
                    }

                    foreach (var error in errors)
                    {
                        if (error.Contains("Nauczyciel"))
                        {
                            points += 50;
                            Console.WriteLine($"  +50 (konflikt nauczyciela): {error}");
                        }
                        // ✅ NOWE: Kara za konflikt klasy
                        else if (error.Contains("lekcje jednocześnie"))
                        {
                            points += 50;
                            Console.WriteLine($"  +50 (konflikt klasy): {error}");
                        }
                        else if (error.Contains("Luka"))
                        {
                            points += breaksWeight;
                            Console.WriteLine($"  +{breaksWeight} (luka): {error}");
                        }
                        else if (error.Contains("Przekroczono limit godzin"))
                        {
                            points += maxHoursWeight;
                            Console.WriteLine($"  +{maxHoursWeight} (przekroczenie limitu dziennego): {error}");
                        }
                        else if (error.Contains("Sala"))
                        {
                            points += 30;
                            Console.WriteLine($"  +30 (konflikt sali): {error}");
                        }
                        else if (error.Contains("Przekroczono limit przedmiotu"))
                        {
                            string subjectName = ExtractSubjectNameFromError(error);
                            
                            if (!string.IsNullOrEmpty(subjectName) && subjectWeights.ContainsKey(subjectName))
                            {
                                int weight = subjectWeights[subjectName];
                                points += weight;
                                Console.WriteLine($"  +{weight} (przekroczenie limitu '{subjectName}'): {error}");
                            }
                            else
                            {
                                points += 25;
                                Console.WriteLine($"  +25 (przekroczenie limitu [domyślna waga]): {error}");
                            }
                        }
                        else if (error.Contains("Nieprawidłowa liczba"))
                        {
                            points += 300;
                            Console.WriteLine($"  +300 (nieprawidłowa suma tygodniowa): {error}");
                        }
                    }
                }

                Console.WriteLine($"SUMA PUNKTÓW KARNYCH: {points}");
                return points;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd obliczania punktów karnych: {ex.Message}");
                return CalculateDefaultPenalties(errors);
            }
        }

        private string ExtractSubjectNameFromError(string error)
        {
            try
            {
                int start = error.IndexOf("przedmiotu '") + 12;
                int end = error.IndexOf("'", start);
                
                if (start > 11 && end > start)
                {
                    string subjectName = error.Substring(start, end - start);
                    Console.WriteLine($"Wyciągnięto nazwę przedmiotu: '{subjectName}' z błędu");
                    return subjectName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd wyciągania nazwy przedmiotu: {ex.Message}");
            }
            
            return null;
        }

        private int CalculateDefaultPenalties(List<string> errors)
        {
            int points = 0;

            foreach (var error in errors)
            {
                if (error.Contains("Nauczyciel"))
                    points += 50;
                else if (error.Contains("lekcje jednocześnie")) // ✅ NOWE
                    points += 50;
                else if (error.Contains("Luka"))
                    points += 40;
                else if (error.Contains("Przekroczono limit godzin"))
                    points += 35;
                else if (error.Contains("Sala"))
                    points += 30;
                else if (error.Contains("Przekroczono limit przedmiotu"))
                    points += 25;
                else if (error.Contains("Nieprawidłowa liczba"))
                    points += 300;
            }

            Console.WriteLine($"SUMA PUNKTÓW KARNYCH (domyślne): {points}");
            return points;
        }

        private string GetJsonStringProperty(JsonElement element, string propertyName)
        {
            if (element.ValueKind == JsonValueKind.Object && 
                element.TryGetProperty(propertyName, out JsonElement value))
            {
                return value.GetString();
            }
            return null;
        }

        private Dictionary<string, List<PlansModel>> ConvertPythonToPlansModel(List<List<List<JsonElement>>> pythonData, string itemName, string itemType)
        {
            var result = new Dictionary<string, List<PlansModel>>();

            for (int day = 0; day < pythonData.Count && day < _daysOfWeek.Length; day++)
            {
                string dayName = _daysOfWeek[day];
                result[dayName] = new List<PlansModel>();

                for (int hour = 0; hour < pythonData[day].Count && hour < _hourPlan.Count; hour++)
                {
                    string hourTime = _hourPlan[hour];

                    foreach (var lessonElement in pythonData[day][hour])
                    {
                        try
                        {
                            var lesson = new PlansModel
                            {
                                dzien = dayName,
                                godzina = hourTime
                            };

                            if (itemType == "Class")
                            {
                                lesson.przedmiot = GetJsonProperty(lessonElement, "subject");
                                lesson.nauczyciel = GetJsonProperty(lessonElement, "teacher");
                                lesson.sala = GetJsonProperty(lessonElement, "room");
                                lesson.klasa = itemName;
                            }
                            else if (itemType == "Teacher")
                            {
                                lesson.przedmiot = GetJsonProperty(lessonElement, "subject");
                                lesson.klasa = GetJsonProperty(lessonElement, "class");
                                lesson.sala = GetJsonProperty(lessonElement, "room");
                                lesson.nauczyciel = itemName;
                            }
                            else if (itemType == "Room")
                            {
                                lesson.przedmiot = GetJsonProperty(lessonElement, "subject");
                                lesson.nauczyciel = GetJsonProperty(lessonElement, "teacher");
                                lesson.klasa = GetJsonProperty(lessonElement, "class");
                                lesson.sala = itemName;
                            }

                            result[dayName].Add(lesson);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Błąd parsowania lekcji: {ex.Message}");
                        }
                    }
                }
            }

            return result;
        }

        private string GetJsonProperty(JsonElement element, string propertyName)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out JsonElement value))
            {
                return value.GetString();
            }
            return null;
        }

        private void ShowDetails(PlanItem item)
        {
            if (item == null) return;
            
            if (item.Type == "Error" || item.Type == "ErrorGroup" || item.Type == "Summary" || item.Type == "Success")
                return;

            try
            {
                Dictionary<string, List<PlansModel>> scheduleData = null;
                string title = "";

                if (item.Type == "Class" && _rawClassData != null && _rawClassData.ContainsKey(item.Name))
                {
                    scheduleData = ConvertPythonToPlansModel(_rawClassData[item.Name], item.Name, "Class");
                    title = $"Plan klasy: {item.Name}";
                }
                else if (item.Type == "Teacher" && _rawTeacherData != null && _rawTeacherData.ContainsKey(item.Name))
                {
                    scheduleData = ConvertPythonToPlansModel(_rawTeacherData[item.Name], item.Name, "Teacher");
                    title = $"Plan nauczyciela: {item.Name}";
                }
                else if (item.Type == "Room" && _rawRoomData != null && _rawRoomData.ContainsKey(item.Name))
                {
                    scheduleData = ConvertPythonToPlansModel(_rawRoomData[item.Name], item.Name, "Room");
                    title = $"Plan sali: {item.Name}";
                }

                if (scheduleData != null)
                {
                    var detailWindow = new PlanDetailWindow(item.Name, title, scheduleData);
                    bool? result = detailWindow.ShowDialog();

                    if (result == true)
                    {
                        Console.WriteLine("Plan został zaktualizowany - przeładowuję dane...");
                        
                        ReloadAllData();
                        OnSelectedViewTypeChanged();
                        
                        Console.WriteLine("Dane odświeżone");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd szczegółów: {ex.Message}");
                MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class PlanItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Icon { get; set; }
    }
}