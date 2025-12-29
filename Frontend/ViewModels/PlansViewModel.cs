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
        private readonly string[] _daysOfWeek = { "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek", "Sobota" };

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
                        Items.Add(new PlanItem { Name = className, Type = "Class", Icon = "🎓" });
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
                        Items.Add(new PlanItem { Name = teacherName, Type = "Teacher", Icon = "👨‍🏫" });
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
                        Items.Add(new PlanItem { Name = roomName, Type = "Room", Icon = "🚪" });
                        Console.WriteLine($"Dodano salę: {roomName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BŁĄD wczytywania sal: {ex.Message}");
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

                var days = new[] { "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek", "Sobota" };

                var jsonString = File.ReadAllText(classFilePath);
                using (JsonDocument document = JsonDocument.Parse(jsonString))
                {
                    // 1. KONFLIKTY NAUCZYCIELI I SAL
                    for (int dayIndex = 0; dayIndex < days.Length; dayIndex++)
                    {
                        string dayName = days[dayIndex];

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
                                var teacherGroups = lessonsAtThisTime.GroupBy(l => l.teacher).Where(g => g.Count() > 1);
                                foreach (var group in teacherGroups)
                                {
                                    var lessons = group.ToList();
                                    string classes = string.Join(", ", lessons.Select(l => l.className));
                                    allErrors.Add($"🧑‍🏫 Nauczyciel '{group.Key}' ma zajęcia w {lessons.Count} klasach: {classes} ({dayName} {hourTime})");
                                }

                                var roomGroups = lessonsAtThisTime.GroupBy(l => l.room).Where(g => g.Count() > 1);
                                foreach (var group in roomGroups)
                                {
                                    var lessons = group.ToList();
                                    string classes = string.Join(", ", lessons.Select(l => l.className));
                                    allErrors.Add($"🚪 Sala '{group.Key}' zajęta przez {lessons.Count} klasy: {classes} ({dayName} {hourTime})");
                                }
                            }
                        }
                    }

                    // 2. LUKI/DZIURY W PLANACH KLAS
                    // 3. LIMIT GODZIN DZIENNIE
                    // 4. SUMA GODZIN W TYGODNIU
                    foreach (JsonProperty planProperty in document.RootElement.EnumerateObject())
                    {
                        string className = planProperty.Name;

                        if (planProperty.Value.ValueKind != JsonValueKind.Array)
                            continue;

                        var daysArray = planProperty.Value;

                        int totalWeeklyHours = 0;

                        for (int dayIndex = 0; dayIndex < Math.Min(daysArray.GetArrayLength(), days.Length); dayIndex++)
                        {
                            string dayName = days[dayIndex];
                            var daySchedule = daysArray[dayIndex];

                            // Luki
                            var gaps = FindGapsInDay(daySchedule, className, dayName);
                            allErrors.AddRange(gaps);

                            // Liczba godzin w dniu
                            int dailyHours = CountHoursInDay(daySchedule);
                            totalWeeklyHours += dailyHours;

                            // Sprawdź limit dzienny
                            int maxHoursPerDay = GetMaxHoursPerDay();
                            if (dailyHours > maxHoursPerDay)
                            {
                                allErrors.Add($"📅 Przekroczono limit godzin: Klasa '{className}' ma {dailyHours} godzin w {dayName} (max: {maxHoursPerDay})");
                            }
                        }

                        // Sprawdź sumę tygodniową
                        int expectedWeeklyHours = GetExpectedWeeklyHours(className);
                        if (expectedWeeklyHours > 0 && totalWeeklyHours != expectedWeeklyHours)
                        {
                            string diff = totalWeeklyHours > expectedWeeklyHours ?
                                $"+{totalWeeklyHours - expectedWeeklyHours}" :
                                $"{totalWeeklyHours - expectedWeeklyHours}";

                            allErrors.Add($"📊 Nieprawidłowa liczba godzin: Klasa '{className}' ma {totalWeeklyHours} godzin (oczekiwano: {expectedWeeklyHours}, różnica: {diff})");
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
                            if (doc.RootElement.TryGetProperty("max_hours_per_day", out JsonElement maxHours))
                            {
                                return maxHours.GetInt32();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd wczytywania max_hours_per_day: {ex.Message}");
            }

            // Domyślnie 8 godzin
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

                // Wczytaj subjects.json
                string subjectsPath = Path.Combine(dir.FullName, "subjects.json");

                if (!File.Exists(subjectsPath))
                    return 0;

                var json = File.ReadAllText(subjectsPath);
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    int totalHours = 0;

                    foreach (JsonElement subjectElement in doc.RootElement.EnumerateArray())
                    {
                        // Sprawdź czy przedmiot jest przypisany do tej klasy
                        if (subjectElement.TryGetProperty("ClassTypes", out JsonElement classTypes))
                        {
                            bool isForThisClass = false;

                            foreach (JsonElement classType in classTypes.EnumerateArray())
                            {
                                string classTypeName = classType.GetString();

                                // Sprawdź czy klasa pasuje (np. "Informatyka 1" zawiera "Informatyka")
                                if (className.StartsWith(classTypeName))
                                {
                                    isForThisClass = true;
                                    break;
                                }
                            }

                            if (isForThisClass)
                            {
                                if (subjectElement.TryGetProperty("Hours", out JsonElement hours))
                                {
                                    totalHours += hours.GetInt32();
                                }
                            }
                        }
                    }

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
                // Zbierz indeksy godzin gdzie są zajęcia
                var occupiedHours = new List<int>();

                for (int hourIndex = 0; hourIndex < Math.Min(daySchedule.GetArrayLength(), _hourPlan.Count); hourIndex++)
                {
                    var hourLessons = daySchedule[hourIndex];

                    if (hourLessons.GetArrayLength() > 0)
                    {
                        occupiedHours.Add(hourIndex);
                    }
                }

                // Jeśli klasa nie ma żadnych zajęć w tym dniu - nie ma luk
                if (occupiedHours.Count == 0)
                    return gaps;

                // Sprawdź czy są luki między pierwszą a ostatnią lekcją
                int firstLesson = occupiedHours.Min();
                int lastLesson = occupiedHours.Max();

                for (int hour = firstLesson + 1; hour < lastLesson; hour++)
                {
                    if (!occupiedHours.Contains(hour))
                    {
                        // Znaleziono lukę!
                        string gapHour = _hourPlan[hour];
                        gaps.Add($"⏰ Luka w planie: Klasa '{className}' ma przerwę {gapHour} ({dayName})");
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

                // PODSUMOWANIE
                Items.Add(new PlanItem
                {
                    Name = $"═══════════════════════════════════",
                    Type = "Separator",
                    Icon = ""
                });

                Items.Add(new PlanItem
                {
                    Name = $"📊 PODSUMOWANIE BŁĘDÓW",
                    Type = "Header",
                    Icon = "📊"
                });

                // Policz typy błędów
                int teacherConflicts = errors.Count(e => e.Contains("🧑‍🏫 Nauczyciel"));
                int roomConflicts = errors.Count(e => e.Contains("🚪 Sala"));
                int gaps = errors.Count(e => e.Contains("⏰ Luka"));
                int dailyLimit = errors.Count(e => e.Contains("📅 Przekroczono"));
                int weeklyHours = errors.Count(e => e.Contains("📊 Nieprawidłowa"));

                Items.Add(new PlanItem
                {
                    Name = $"Konfliktów nauczycieli: {teacherConflicts}",
                    Type = "Summary",
                    Icon = "🧑‍🏫"
                });

                Items.Add(new PlanItem
                {
                    Name = $"Konfliktów sal: {roomConflicts}",
                    Type = "Summary",
                    Icon = "🚪"
                });

                Items.Add(new PlanItem
                {
                    Name = $"Luk w planach: {gaps}",
                    Type = "Summary",
                    Icon = "⏰"
                });

                Items.Add(new PlanItem
                {
                    Name = $"Przekroczeń limitu dziennego: {dailyLimit}",
                    Type = "Summary",
                    Icon = "📅"
                });

                Items.Add(new PlanItem
                {
                    Name = $"Błędów sumy tygodniowej: {weeklyHours}",
                    Type = "Summary",
                    Icon = "📊"
                });

                Items.Add(new PlanItem
                {
                    Name = $"Łącznie problemów: {errors.Count}",
                    Type = "Summary",
                    Icon = "🔢"
                });

                int penaltyPoints = CalculatePenaltyPoints(errors);

                Items.Add(new PlanItem
                {
                    Name = $"⚠️ Punkty karne: {penaltyPoints}",
                    Type = "Summary",
                    Icon = "⚠️"
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
                        Name = "✅ Brak błędów! Plan jest idealny!",
                        Type = "Success",
                        Icon = "✅"
                    });
                }
                else
                {
                    // GRUPY BŁĘDÓW
                    if (teacherConflicts > 0)
                    {
                        Items.Add(new PlanItem { Name = $"🧑‍🏫 KONFLIKTY NAUCZYCIELI ({teacherConflicts}):", Type = "Header", Icon = "🧑‍🏫" });
                        AddErrorsToList(errors, "🧑‍🏫 Nauczyciel");
                    }

                    if (roomConflicts > 0)
                    {
                        Items.Add(new PlanItem { Name = $"🚪 KONFLIKTY SAL ({roomConflicts}):", Type = "Header", Icon = "🚪" });
                        AddErrorsToList(errors, "🚪 Sala");
                    }

                    if (gaps > 0)
                    {
                        Items.Add(new PlanItem { Name = $"⏰ LUKI W PLANACH ({gaps}):", Type = "Header", Icon = "⏰" });
                        AddErrorsToList(errors, "⏰ Luka");
                    }

                    if (dailyLimit > 0)
                    {
                        Items.Add(new PlanItem { Name = $"📅 PRZEKROCZENIA LIMITU DZIENNEGO ({dailyLimit}):", Type = "Header", Icon = "📅" });
                        AddErrorsToList(errors, "📅 Przekroczono");
                    }

                    if (weeklyHours > 0)
                    {
                        Items.Add(new PlanItem { Name = $"📊 BŁĘDY SUMY GODZIN ({weeklyHours}):", Type = "Header", Icon = "📊" });
                        AddErrorsToList(errors, "📊 Nieprawidłowa");
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
                    Icon = "❌"
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
            // HIERARCHIA WAG BŁĘDÓW:
            // 1. Konflikt nauczyciela (-50) - KRYTYCZNY
            // 2. Luka w planie (-40) - BARDZO ZŁY
            // 3. Przekroczenie limitu godzin (-35) - BARDZO ZŁY
            // 4. Konflikt sali (-30) - ŹLE
            // 5. Nieprawidłowa suma godzin (-300) - ŚREDNIO ZŁY

            int points = 0;

            foreach (var error in errors)
            {
                if (error.Contains("🧑‍🏫 Nauczyciel"))
                    points += 50;
                else if (error.Contains("⏰ Luka"))
                    points += 40;
                else if (error.Contains("📅 Przekroczono limit"))
                    points += 35;
                else if (error.Contains("🚪 Sala"))
                    points += 30;
                else if (error.Contains("📊 Nieprawidłowa liczba"))
                    points += 300;
            }

            return points;
        }





        // private List<string> AnalyzeAllConflicts()
        // {
        //     var allErrors = new List<string>();
            
        //     try
        //     {
        //         string currentPath = AppDomain.CurrentDomain.BaseDirectory;
        //         DirectoryInfo dir = new DirectoryInfo(currentPath);
                
        //         while (dir != null && dir.Name != "Inzynierka")
        //         {
        //             dir = dir.Parent;
        //         }
                
        //         if (dir == null) return allErrors;

        //         string plansPath = Path.Combine(dir.FullName, "Backend", "algotithm", "plany");
        //         string classFilePath = Path.Combine(plansPath, "class_schedules.json");
                
        //         if (!File.Exists(classFilePath))
        //             return allErrors;

        //         var days = new[] { "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek", "Sobota" };

        //         var jsonString = File.ReadAllText(classFilePath);
        //         using (JsonDocument document = JsonDocument.Parse(jsonString))
        //         {
        //             for (int dayIndex = 0; dayIndex < days.Length; dayIndex++)
        //             {
        //                 string dayName = days[dayIndex];
                        
        //                 for (int hourIndex = 0; hourIndex < _hourPlan.Count; hourIndex++)
        //                 {
        //                     string hourTime = _hourPlan[hourIndex];
                            
        //                     var lessonsAtThisTime = new List<(string className, string teacher, string room, string subject)>();
                            
        //                     foreach (JsonProperty planProperty in document.RootElement.EnumerateObject())
        //                     {
        //                         string className = planProperty.Name;
                                
        //                         if (planProperty.Value.ValueKind != JsonValueKind.Array)
        //                             continue;

        //                         var daysArray = planProperty.Value;
                                
        //                         if (dayIndex >= daysArray.GetArrayLength())
        //                             continue;

        //                         var daySchedule = daysArray[dayIndex];
                                
        //                         if (hourIndex >= daySchedule.GetArrayLength())
        //                             continue;

        //                         var hourLessons = daySchedule[hourIndex];

        //                         foreach (JsonElement lessonElement in hourLessons.EnumerateArray())
        //                         {
        //                             string teacher = GetJsonStringProperty(lessonElement, "teacher");
        //                             string room = GetJsonStringProperty(lessonElement, "room");
        //                             string subject = GetJsonStringProperty(lessonElement, "subject");
                                    
        //                             lessonsAtThisTime.Add((className, teacher, room, subject));
        //                         }
        //                     }
                            
        //                     if (lessonsAtThisTime.Count > 1)
        //                     {
        //                         var teacherGroups = lessonsAtThisTime.GroupBy(l => l.teacher).Where(g => g.Count() > 1);
        //                         foreach (var group in teacherGroups)
        //                         {
        //                             var lessons = group.ToList();
        //                             string classes = string.Join(", ", lessons.Select(l => l.className));
        //                             allErrors.Add($"🧑‍🏫 Nauczyciel '{group.Key}' ma zajęcia w {lessons.Count} klasach: {classes} ({dayName} {hourTime})");
        //                         }
                                
        //                         var roomGroups = lessonsAtThisTime.GroupBy(l => l.room).Where(g => g.Count() > 1);
        //                         foreach (var group in roomGroups)
        //                         {
        //                             var lessons = group.ToList();
        //                             string classes = string.Join(", ", lessons.Select(l => l.className));
        //                             allErrors.Add($"🚪 Sala '{group.Key}' zajęta przez {lessons.Count} klasy: {classes} ({dayName} {hourTime})");
        //                         }
        //                     }
        //                 }
        //             }
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Błąd analizy: {ex.Message}");
        //     }
            
        //     return allErrors;
        // }

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
            
            // Nie otwieraj szczegółów dla błędów
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
                        LoadData();
                        OnSelectedViewTypeChanged(); // Odśwież aktualny widok
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