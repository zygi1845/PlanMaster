using Frontend.Models;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;
using System.Windows.Controls;

namespace Frontend.Views
{
    public partial class PlanDetailWindow : Window
    {
        public new string Title { get; set; }
        public string PlanTitle { get; set; }
        public ObservableCollection<HourRow> Hours { get; set; }
        private Dictionary<string, List<PlansModel>> _originalScheduleData;
        private Dictionary<string, List<PlansModel>> _modifiedScheduleData;
        private bool _hasChanges = false;
        private PlansModel _draggedLesson;
        private Point _startPoint;

        public PlanDetailWindow(string itemName, string title, Dictionary<string, List<PlansModel>> scheduleData)
        {
            InitializeComponent();

            this.Title = title;
            this.PlanTitle = title;
            Hours = new ObservableCollection<HourRow>();

            _originalScheduleData = DeepCopySchedule(scheduleData);
            _modifiedScheduleData = DeepCopySchedule(scheduleData);

            var hoursList = LoadHourPlan();

            var dayMap = new Dictionary<string, int>
            {
                { "Poniedziałek", 0 },
                { "Wtorek", 1 },
                { "Środa", 2 },
                { "Czwartek", 3 },
                { "Piątek", 4 }
            };

            foreach (var hourTime in hoursList)
            {
                var row = new HourRow { Hour = hourTime };

                foreach (var day in dayMap.Keys)
                {
                    var lessons = _modifiedScheduleData.ContainsKey(day)
                        ? _modifiedScheduleData[day].Where(l => l.godzina == hourTime).ToList()
                        : new List<PlansModel>();

                    switch (dayMap[day])
                    {
                        case 0: row.Monday = new ObservableCollection<PlansModel>(lessons); break;
                        case 1: row.Tuesday = new ObservableCollection<PlansModel>(lessons); break;
                        case 2: row.Wednesday = new ObservableCollection<PlansModel>(lessons); break;
                        case 3: row.Thursday = new ObservableCollection<PlansModel>(lessons); break;
                        case 4: row.Friday = new ObservableCollection<PlansModel>(lessons); break;
                    }
                }

                Hours.Add(row);
            }

            DataContext = this;
            MarkAllConflicts();
        }

        private Dictionary<string, List<PlansModel>> DeepCopySchedule(Dictionary<string, List<PlansModel>> original)
        {
            var copy = new Dictionary<string, List<PlansModel>>();
            foreach (var kvp in original)
            {
                copy[kvp.Key] = new List<PlansModel>(kvp.Value.Select(l => new PlansModel
                {
                    dzien = l.dzien,
                    godzina = l.godzina,
                    przedmiot = l.przedmiot,
                    nauczyciel = l.nauczyciel,
                    sala = l.sala,
                    klasa = l.klasa
                }));
            }
            return copy;
        }

        public void OnLessonDrop(PlansModel lesson, string targetDay, string targetHour)
        {
            if (lesson == null) return;

            string oldDay = lesson.dzien;
            string oldHour = lesson.godzina;

            var conflicts = CheckConflicts(lesson, targetDay, targetHour);

            if (conflicts.Count > 0)
            {
                string conflictMessage = "⚠️ WYKRYTO KONFLIKTY:\n\n";
                foreach (var conflict in conflicts)
                {
                    conflictMessage += $"❌ {conflict}\n";
                }
                conflictMessage += "\n🤔 Czy mimo to chcesz przenieść lekcję?";

                var result = MessageBox.Show(conflictMessage, "Konflikt wykryty",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            MoveLessonInData(lesson, oldDay, oldHour, targetDay, targetHour);
            RefreshView();
            MarkAllConflicts();
            _hasChanges = true;

            MessageBox.Show($"✅ Przeniesiono lekcję:\n\n" +
                          $"📚 {lesson.przedmiot}\n" +
                          $"👨‍🏫 {lesson.nauczyciel}\n\n" +
                          $"Z: {oldDay} {oldHour}\n" +
                          $"Do: {targetDay} {targetHour}\n\n" +
                          $"💡 Kliknij 'Zatwierdź' aby zapisać zmiany.",
                          "Lekcja przeniesiona", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private List<string> CheckConflicts(PlansModel lesson, string targetDay, string targetHour)
        {
            var conflicts = new List<string>();

            if (_modifiedScheduleData.ContainsKey(targetDay))
            {
                var lessonsAtTarget = _modifiedScheduleData[targetDay]
                    .Where(l => l.godzina == targetHour)
                    .ToList();

                foreach (var existingLesson in lessonsAtTarget)
                {
                    if (existingLesson == lesson)
                    {
                        Console.WriteLine($"  Pomijam tę samą lekcję: {lesson.przedmiot}");
                        continue;
                    }

                    if (existingLesson.przedmiot == lesson.przedmiot &&
                        existingLesson.nauczyciel == lesson.nauczyciel &&
                        existingLesson.sala == lesson.sala &&
                        existingLesson.klasa == lesson.klasa)
                    {
                        Console.WriteLine($"  Pomijam tę samą lekcję (po polach): {lesson.przedmiot}");
                        continue;
                    }

                    if (PlanTitle.Contains("Plan klasy:") && existingLesson.klasa == lesson.klasa)
                    {
                        conflicts.Add($"Klasa '{lesson.klasa}' ma już zajęcia: {existingLesson.przedmiot} ({existingLesson.nauczyciel})");
                    }
                }
            }

            try
            {
                var globalConflicts = CheckGlobalConflicts(lesson, targetDay, targetHour);
                conflicts.AddRange(globalConflicts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd sprawdzania konfliktów globalnych: {ex.Message}");
            }

            return conflicts;
        }

        private List<string> CheckGlobalConflicts(PlansModel lesson, string targetDay, string targetHour)
        {
            var conflicts = new List<string>();
            var seenConflicts = new HashSet<string>();

            try
            {
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo dir = new DirectoryInfo(currentPath);

                while (dir != null && dir.Name != "Inzynierka")
                {
                    dir = dir.Parent;
                }

                if (dir == null) return conflicts;

                string plansPath = Path.Combine(dir.FullName, "Backend", "algotithm", "plany");

                var dayIndexMap = new Dictionary<string, int>
                {
                    { "Poniedziałek", 0 },
                    { "Wtorek", 1 },
                    { "Środa", 2 },
                    { "Czwartek", 3 },
                    { "Piątek", 4 },
                    { "Sobota", 5 }
                };

                if (!dayIndexMap.ContainsKey(targetDay))
                    return conflicts;

                int dayIndex = dayIndexMap[targetDay];
                var hours = LoadHourPlan();
                int hourIndex = hours.IndexOf(targetHour);

                if (hourIndex < 0)
                    return conflicts;

                string filePath = Path.Combine(plansPath, "class_schedules.json");

                if (!File.Exists(filePath))
                    return conflicts;

                var jsonString = File.ReadAllText(filePath);
                using (JsonDocument document = JsonDocument.Parse(jsonString))
                {
                    foreach (JsonProperty planProperty in document.RootElement.EnumerateObject())
                    {
                        string planName = planProperty.Name;

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

                            if (planName == lesson.klasa &&
                                teacher == lesson.nauczyciel &&
                                subject == lesson.przedmiot &&
                                room == lesson.sala)
                            {
                                continue;
                            }

                            if (!string.IsNullOrEmpty(teacher) && teacher == lesson.nauczyciel)
                            {
                                string conflictKey = $"teacher_{teacher}_{planName}_{subject}";
                                if (!seenConflicts.Contains(conflictKey))
                                {
                                    conflicts.Add($"🧑‍🏫 Nauczyciel '{teacher}' ma już zajęcia: {subject} w klasie {planName}");
                                    seenConflicts.Add(conflictKey);
                                }
                            }

                            if (!string.IsNullOrEmpty(room) && room == lesson.sala)
                            {
                                string conflictKey = $"room_{room}_{planName}_{subject}_{teacher}";
                                if (!seenConflicts.Contains(conflictKey))
                                {
                                    conflicts.Add($"🚪 Sala '{room}' jest już zajęta: {subject} (Nauczyciel: {teacher ?? "?"}, Klasa: {planName})");
                                    seenConflicts.Add(conflictKey);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd CheckGlobalConflicts: {ex.Message}");
            }

            return conflicts;
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

        private void MoveLessonInData(PlansModel lesson, string oldDay, string oldHour, string newDay, string newHour)
        {
            if (_modifiedScheduleData.ContainsKey(oldDay))
            {
                var oldLesson = _modifiedScheduleData[oldDay].FirstOrDefault(l => l == lesson);
                
                if (oldLesson != null)
                {
                    _modifiedScheduleData[oldDay].Remove(oldLesson);
                    Console.WriteLine($"✓ Usunięto lekcję: {oldLesson.przedmiot} z {oldDay} {oldHour}");
                }
                else
                {
                    oldLesson = _modifiedScheduleData[oldDay]
                        .FirstOrDefault(l => l.godzina == oldHour &&
                                           l.przedmiot == lesson.przedmiot &&
                                           l.nauczyciel == lesson.nauczyciel &&
                                           l.sala == lesson.sala &&
                                           l.klasa == lesson.klasa);
                    
                    if (oldLesson != null)
                    {
                        _modifiedScheduleData[oldDay].Remove(oldLesson);
                        Console.WriteLine($"✓ Usunięto lekcję (fallback): {oldLesson.przedmiot} z {oldDay} {oldHour}");
                    }
                }
            }

            if (!_modifiedScheduleData.ContainsKey(newDay))
            {
                _modifiedScheduleData[newDay] = new List<PlansModel>();
            }

            lesson.dzien = newDay;
            lesson.godzina = newHour;

            _modifiedScheduleData[newDay].Add(lesson);
            
            Console.WriteLine($"✓ Dodano lekcję: {lesson.przedmiot} do {newDay} {newHour}");
        }

        private void RefreshView()
        {
            Hours.Clear();

            var hoursList = LoadHourPlan();
            var dayMap = new Dictionary<string, int>
            {
                { "Poniedziałek", 0 },
                { "Wtorek", 1 },
                { "Środa", 2 },
                { "Czwartek", 3 },
                { "Piątek", 4 }
            };

            foreach (var hourTime in hoursList)
            {
                var row = new HourRow { Hour = hourTime };

                foreach (var day in dayMap.Keys)
                {
                    var lessons = _modifiedScheduleData.ContainsKey(day)
                        ? _modifiedScheduleData[day].Where(l => l.godzina == hourTime).ToList()
                        : new List<PlansModel>();

                    switch (dayMap[day])
                    {
                        case 0: row.Monday = new ObservableCollection<PlansModel>(lessons); break;
                        case 1: row.Tuesday = new ObservableCollection<PlansModel>(lessons); break;
                        case 2: row.Wednesday = new ObservableCollection<PlansModel>(lessons); break;
                        case 3: row.Thursday = new ObservableCollection<PlansModel>(lessons); break;
                        case 4: row.Friday = new ObservableCollection<PlansModel>(lessons); break;
                    }
                }

                Hours.Add(row);
            }
        }

        private List<string> LoadHourPlan()
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
                            var hours = new List<string>();
                            foreach (var item in doc.RootElement.EnumerateArray())
                            {
                                if (item.TryGetProperty("Hour", out JsonElement hourElement))
                                {
                                    hours.Add(hourElement.GetString());
                                }
                            }
                            return hours;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd wczytywania hourplan.json: {ex.Message}");
            }

            return new List<string>
            {
                "8:00-8:45", "8:50-9:35", "9:40-10:25", "10:30-11:15",
                "11:20-12:05", "12:10-12:55", "13:00-13:45", "13:50-14:35"
            };
        }

        // EVENT HANDLERY DLA DRAG & DROP
        private void LessonBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void LessonBlock_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = _startPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var border = sender as Border;
                    _draggedLesson = border?.DataContext as PlansModel;

                    if (_draggedLesson != null)
                    {
                        DataObject dragData = new DataObject("lesson", _draggedLesson);
                        DragDrop.DoDragDrop(border, dragData, DragDropEffects.Move);
                    }
                }
            }
        }

        private void LessonCell_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("lesson"))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void LessonCell_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("lesson"))
            {
                var lesson = e.Data.GetData("lesson") as PlansModel;
                var cell = sender as Border;

                string targetDay = cell.Tag as string;

                var grid = cell.Parent as Grid;
                if (grid != null)
                {
                    var hourRow = grid.DataContext as HourRow;
                    if (hourRow != null)
                    {
                        OnLessonDrop(lesson, targetDay, hourRow.Hour);
                    }
                }
            }
        }

        // UZUPEŁNIANIE BRAKÓW
        private void FillMissing_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var missingSubjects = FindMissingSubjects();

                if (missingSubjects.Count == 0)
                {
                    MessageBox.Show("✅ Wszystkie przedmioty są kompletne!\n\nBrak brakujących godzin.",
                        "Brak problemów", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var message = "📋 BRAKUJĄCE PRZEDMIOTY:\n\n";
                foreach (var missing in missingSubjects)
                {
                    message += $"• {missing.SubjectName}: brakuje {missing.MissingHours} godzin\n";
                    message += $"  (ma {missing.CurrentHours}, powinno być {missing.ExpectedHours})\n\n";
                }
                message += "❓ Czy chcesz automatycznie uzupełnić te przedmioty?";

                var result = MessageBox.Show(message, "Uzupełnij brakujące przedmioty",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    int added = AutoFillMissingLessons(missingSubjects);

                    RefreshView();
                    MarkAllConflicts();
                    _hasChanges = true;

                    MessageBox.Show($"✅ Dodano {added} lekcji!\n\n💡 Sprawdź plan i kliknij 'Zatwierdź' aby zapisać zmiany.",
                        "Uzupełniono", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ BŁĄD:\n\n{ex.Message}\n\nStack:\n{ex.StackTrace}",
                    "Błąd krytyczny", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<MissingSubjectInfo> FindMissingSubjects()
        {
            var missing = new List<MissingSubjectInfo>();

            try
            {
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo dir = new DirectoryInfo(currentPath);

                while (dir != null && dir.Name != "Inzynierka")
                {
                    dir = dir.Parent;
                }

                if (dir == null) return missing;

                string subjectsPath = Path.Combine(dir.FullName, "subjects.json");
                if (!File.Exists(subjectsPath)) return missing;

                string className = PlanTitle.Contains(":") ? PlanTitle.Split(':')[1].Trim() : "";
                if (string.IsNullOrEmpty(className)) return missing;

                var json = File.ReadAllText(subjectsPath);
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    string baseClassName = className;
                    int lastSpace = className.LastIndexOf(' ');
                    if (lastSpace > 0 && int.TryParse(className.Substring(lastSpace + 1), out _))
                    {
                        baseClassName = className.Substring(0, lastSpace).Trim();
                    }

                    Console.WriteLine($"\nAnaliza braków dla klasy: '{className}' (typ: '{baseClassName}')");

                    foreach (JsonElement subjectElement in doc.RootElement.EnumerateArray())
                    {
                        if (!subjectElement.TryGetProperty("ClassTypes", out JsonElement classTypes))
                            continue;

                        bool isForThisClass = false;
                        foreach (JsonElement classType in classTypes.EnumerateArray())
                        {
                            if (classType.GetString() == baseClassName)
                            {
                                isForThisClass = true;
                                break;
                            }
                        }

                        if (!isForThisClass) continue;

                        string subjectName = subjectElement.TryGetProperty("Name", out JsonElement name)
                            ? name.GetString() : "";
                        int expectedHours = subjectElement.TryGetProperty("Hours", out JsonElement hours)
                            ? hours.GetInt32() : 0;

                        int currentHours = 0;
                        var validDays = new[] { "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek" };
                        
                        foreach (var day in _modifiedScheduleData)
                        {
                            if (!validDays.Contains(day.Key))
                                continue;

                            foreach (var lesson in day.Value)
                            {
                                if (lesson.przedmiot == subjectName && lesson.klasa == className)
                                {
                                    currentHours++;
                                }
                            }
                        }

                        Console.WriteLine($"{subjectName}: ma {currentHours}h, powinno {expectedHours}h");

                        if (currentHours < expectedHours)
                        {
                            Console.WriteLine($"  ⚠️ BRAK: {expectedHours - currentHours}h");
                            
                            missing.Add(new MissingSubjectInfo
                            {
                                SubjectName = subjectName,
                                ExpectedHours = expectedHours,
                                CurrentHours = currentHours,
                                MissingHours = expectedHours - currentHours
                            });
                        }
                    }

                    Console.WriteLine($"\nRAZEM braków: {missing.Count}\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ BŁĄD: {ex.Message}");
                MessageBox.Show($"❌ Błąd:\n{ex.Message}", "Błąd");
            }

            return missing;
        }

        private int AutoFillMissingLessons(List<MissingSubjectInfo> missingSubjects)
        {
            int addedCount = 0;
            var hours = LoadHourPlan();
            var days = new[] { "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek" };

            string className = PlanTitle.Contains(":") ? PlanTitle.Split(':')[1].Trim() : "";

            foreach (var missingInfo in missingSubjects)
            {
                int toAdd = missingInfo.MissingHours;

                string teacherName = FindTeacherForSubject(missingInfo.SubjectName, className);
                if (string.IsNullOrEmpty(teacherName))
                {
                    Console.WriteLine($"⚠️ Brak nauczyciela dla {missingInfo.SubjectName} w klasie {className}");
                    teacherName = "Nieprzypisany";
                }
                else
                {
                    Console.WriteLine($"✓ Znaleziono nauczyciela: {teacherName} dla {missingInfo.SubjectName}");
                }

                string room = FindRoomForSubject(missingInfo.SubjectName);
                if (string.IsNullOrEmpty(room))
                {
                    room = "Do ustalenia";
                }

                Console.WriteLine($"\nDodawanie {toAdd} lekcji '{missingInfo.SubjectName}':");
                Console.WriteLine($"  Nauczyciel: {teacherName}");
                Console.WriteLine($"  Sala: {room}");

                for (int attempt = 0; attempt < toAdd && attempt < 100; attempt++)
                {
                    bool added = false;

                    for (int dayIndex = 0; dayIndex < days.Length && !added; dayIndex++)
                    {
                        string day = days[dayIndex];

                        for (int hourIndex = 0; hourIndex < hours.Count && !added; hourIndex++)
                        {
                            string hour = hours[hourIndex];

                            if (!_modifiedScheduleData.ContainsKey(day))
                            {
                                _modifiedScheduleData[day] = new List<PlansModel>();
                            }

                            var lessonsAtTime = _modifiedScheduleData[day]
                                .Where(l => l.godzina == hour)
                                .ToList();

                            if (lessonsAtTime.Count == 0)
                            {
                                var newLesson = new PlansModel
                                {
                                    przedmiot = missingInfo.SubjectName,
                                    nauczyciel = teacherName,
                                    sala = room,
                                    dzien = day,
                                    godzina = hour,
                                    klasa = className
                                };

                                _modifiedScheduleData[day].Add(newLesson);
                                addedCount++;
                                added = true;

                                Console.WriteLine($"  ✓ Dodano: {day} {hour}");
                            }
                        }
                    }

                    if (!added)
                    {
                        Console.WriteLine($"  ✗ Nie znaleziono wolnego slotu!");
                        break;
                    }
                }
            }

            return addedCount;
        }

        private string FindTeacherForSubject(string subjectName, string className)
        {
            try
            {
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo dir = new DirectoryInfo(currentPath);

                while (dir != null && dir.Name != "Inzynierka")
                {
                    dir = dir.Parent;
                }

                if (dir == null) return null;

                string teachersPath = Path.Combine(dir.FullName, "teachers.json");
                if (!File.Exists(teachersPath)) return null;

                var json = File.ReadAllText(teachersPath);

                Console.WriteLine($"\n🔍 Szukam nauczyciela dla: '{subjectName}' w klasie '{className}'");

                string baseClassName = className;
                int lastSpace = className.LastIndexOf(' ');
                if (lastSpace > 0 && int.TryParse(className.Substring(lastSpace + 1), out _))
                {
                    baseClassName = className.Substring(0, lastSpace).Trim();
                }

                Console.WriteLine($"  Typ klasy (base): '{baseClassName}'");

                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    foreach (JsonElement teacher in doc.RootElement.EnumerateArray())
                    {
                        string teacherName = teacher.GetProperty("Nazwisko").GetString();
                        
                        if (!teacher.TryGetProperty("SelectedSubject", out JsonElement selectedSubject))
                            continue;

                        if (!selectedSubject.TryGetProperty("Name", out JsonElement nameElement))
                            continue;

                        string teacherSubjectName = nameElement.GetString();

                        if (teacherSubjectName == subjectName)
                        {
                            Console.WriteLine($"  ✓ Znaleziono nauczyciela '{teacherName}' z przedmiotem '{subjectName}'");

                            if (selectedSubject.TryGetProperty("ClassTypes", out JsonElement classTypesElement))
                            {
                                foreach (JsonElement classType in classTypesElement.EnumerateArray())
                                {
                                    string ct = classType.GetString();
                                    
                                    if (ct == baseClassName)
                                    {
                                        Console.WriteLine($"  ✅ PASUJE! Nauczyciel '{teacherName}' uczy '{subjectName}' dla '{baseClassName}'");
                                        return teacherName;
                                    }
                                }
                                
                                Console.WriteLine($"  ✗ Nie pasuje klasa (nauczyciel uczy inne klasy)");
                            }
                        }
                    }
                }

                Console.WriteLine($"  ❌ Nie znaleziono nauczyciela");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Błąd FindTeacherForSubject: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                return null;
            }
        }

        private string FindRoomForSubject(string subjectName)
        {
            try
            {
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo dir = new DirectoryInfo(currentPath);

                while (dir != null && dir.Name != "Inzynierka")
                {
                    dir = dir.Parent;
                }

                if (dir == null) return null;

                string subjectsPath = Path.Combine(dir.FullName, "subjects.json");
                if (!File.Exists(subjectsPath)) return null;

                string subjectType = null;
                var subJson = File.ReadAllText(subjectsPath);
                using (JsonDocument doc = JsonDocument.Parse(subJson))
                {
                    foreach (JsonElement subject in doc.RootElement.EnumerateArray())
                    {
                        if (subject.TryGetProperty("Name", out JsonElement name) &&
                            name.GetString() == subjectName)
                        {
                            if (subject.TryGetProperty("SubjectType", out JsonElement type))
                            {
                                subjectType = type.GetString();
                                break;
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(subjectType)) return null;

                string roomsPath = Path.Combine(dir.FullName, "rooms.json");
                if (!File.Exists(roomsPath)) return null;

                var roomJson = File.ReadAllText(roomsPath);
                using (JsonDocument doc = JsonDocument.Parse(roomJson))
                {
                    foreach (JsonElement room in doc.RootElement.EnumerateArray())
                    {
                        if (room.TryGetProperty("RoomType", out JsonElement roomType) &&
                            roomType.GetString() == subjectType)
                        {
                            if (room.TryGetProperty("RoomNumber", out JsonElement roomNum))
                            {
                                return roomNum.GetString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd FindRoomForSubject: {ex.Message}");
            }

            return null;
        }

        private void DeletePlan_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"⚠️ Czy na pewno chcesz USUNĄĆ CAŁKOWICIE ten plan?\n\n" +
                $"Plan: {PlanTitle}\n\n" +
                $"❌ Ta operacja jest NIEODWRACALNA!\n" +
                $"Plan zostanie usunięty z pliku JSON.",
                "USUŃ PLAN",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                bool deleted = DeletePlanFromFile();

                if (deleted)
                {
                    MessageBox.Show("✅ Plan został usunięty!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }

        private bool DeletePlanFromFile()
        {
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
                    MessageBox.Show("Nie znaleziono katalogu Inzynierka!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                string plansPath = Path.Combine(dir.FullName, "Backend", "algotithm", "plany");

                string fileName = "";
                if (PlanTitle.Contains("Plan klasy:"))
                    fileName = "class_schedules.json";
                else if (PlanTitle.Contains("Plan nauczyciela:"))
                    fileName = "teacher_schedules.json";
                else if (PlanTitle.Contains("Plan sali:"))
                    fileName = "room_schedules.json";
                else
                {
                    MessageBox.Show("Nie można określić typu planu!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                string filePath = Path.Combine(plansPath, fileName);

                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"Plik nie istnieje:\n{filePath}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                string itemName = PlanTitle.Contains(":") ? PlanTitle.Split(':')[1].Trim() : "";

                if (string.IsNullOrEmpty(itemName))
                {
                    MessageBox.Show("Nie można odczytać nazwy z tytułu!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                Console.WriteLine($"Usuwam plan: {itemName} z pliku: {fileName}");

                var jsonString = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(jsonString);
                var allSchedulesDict = new Dictionary<string, JsonElement>();

                foreach (JsonProperty property in document.RootElement.EnumerateObject())
                {
                    if (property.Name != itemName)
                    {
                        allSchedulesDict[property.Name] = property.Value.Clone();
                    }
                }

                using (var stream = new MemoryStream())
                {
                    using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
                    {
                        writer.WriteStartObject();

                        foreach (var kvp in allSchedulesDict)
                        {
                            writer.WritePropertyName(kvp.Key);
                            kvp.Value.WriteTo(writer);
                        }

                        writer.WriteEndObject();
                    }

                    var finalJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                    File.WriteAllText(filePath, finalJson);
                }

                document.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Błąd usuwania:\n\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (_hasChanges)
            {
                var result = MessageBox.Show("Masz niezapisane zmiany. Czy na pewno chcesz anulować?",
                    "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;
            }

            this.Close();
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            if (!_hasChanges)
            {
                MessageBox.Show("Brak zmian do zapisania.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                return;
            }

            var result = MessageBox.Show(
                "✅ Czy chcesz zapisać zmiany do pliku?\n\n" +
                "Zostaną zaktualizowane odpowiednie plany.",
                "Zapisz zmiany", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SaveChangesToFile();
                this.DialogResult = true;
            }

            this.Close();
        }

        // ========================================
        // ✅ POPRAWIONA METODA SaveChangesToFile
        // ========================================
        private void SaveChangesToFile()
        {
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
                    MessageBox.Show("Nie znaleziono katalogu Inzynierka!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string plansPath = Path.Combine(dir.FullName, "Backend", "algotithm", "plany");

                bool success = true;

                // ✅ KLUCZOWA POPRAWKA: Sprawdź jaki plan edytujesz
                if (PlanTitle.Contains("Plan klasy:"))
                {
                    // Gdy edytujesz KLASĘ → aktualizuj wszystkie 3 plany
                    Console.WriteLine("Edycja planu klasy - aktualizuję wszystkie plany");
                    success &= UpdateClassSchedules(plansPath);
                    success &= UpdateTeacherSchedules(plansPath);
                    success &= UpdateRoomSchedules(plansPath);
                }
                else if (PlanTitle.Contains("Plan nauczyciela:"))
                {
                    // Gdy edytujesz NAUCZYCIELA → aktualizuj tylko plany nauczyciela
                    Console.WriteLine("Edycja planu nauczyciela - aktualizuję tylko plan nauczyciela");
                    success &= UpdateTeacherSchedulesDirect(plansPath);
                }
                else if (PlanTitle.Contains("Plan sali:"))
                {
                    // Gdy edytujesz SALĘ → aktualizuj tylko plany sali
                    Console.WriteLine("Edycja planu sali - aktualizuję tylko plan sali");
                    success &= UpdateRoomSchedulesDirect(plansPath);
                }

                if (success)
                {
                    string message = "✅ Zapisano zmiany!\n\n";
                    
                    if (PlanTitle.Contains("Plan klasy:"))
                    {
                        message += "✓ Plany klas\n✓ Plany nauczycieli\n✓ Plany sal";
                    }
                    else if (PlanTitle.Contains("Plan nauczyciela:"))
                    {
                        message += "✓ Plan nauczyciela";
                    }
                    else if (PlanTitle.Contains("Plan sali:"))
                    {
                        message += "✓ Plan sali";
                    }
                    
                    MessageBox.Show(message, "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Błąd zapisu:\n\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========================================
        // METODY UPDATE - Dla planu klasy (inkrementalna aktualizacja)
        // ========================================

        private bool UpdateClassSchedules(string plansPath)
        {
            try
            {
                string filePath = Path.Combine(plansPath, "class_schedules.json");
                if (!File.Exists(filePath)) return false;

                var jsonString = File.ReadAllText(filePath);
                using (JsonDocument document = JsonDocument.Parse(jsonString))
                {
                    var allSchedulesDict = new Dictionary<string, JsonElement>();
                    
                    foreach (JsonProperty property in document.RootElement.EnumerateObject())
                    {
                        allSchedulesDict[property.Name] = property.Value.Clone();
                    }

                    var classesInData = new HashSet<string>();
                    foreach (var day in _modifiedScheduleData.Values)
                    {
                        foreach (var lesson in day)
                        {
                            if (!string.IsNullOrEmpty(lesson.klasa))
                                classesInData.Add(lesson.klasa);
                        }
                    }

                    foreach (var className in classesInData)
                    {
                        var classSchedule = BuildClassSchedule(className);
                        var scheduleJson = JsonSerializer.Serialize(classSchedule);
                        var scheduleElement = JsonDocument.Parse(scheduleJson).RootElement.Clone();
                        allSchedulesDict[className] = scheduleElement;

                        Console.WriteLine($"Zaktualizowano plan klasy: {className}");
                    }

                    SaveJsonFile(filePath, allSchedulesDict);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd UpdateClassSchedules: {ex.Message}");
                return false;
            }
        }

        private bool UpdateTeacherSchedules(string plansPath)
        {
            try
            {
                string filePath = Path.Combine(plansPath, "teacher_schedules.json");
                if (!File.Exists(filePath)) return false;

                var jsonString = File.ReadAllText(filePath);
                using (JsonDocument document = JsonDocument.Parse(jsonString))
                {
                    var allSchedulesDict = new Dictionary<string, JsonElement>();
                    
                    foreach (JsonProperty property in document.RootElement.EnumerateObject())
                    {
                        allSchedulesDict[property.Name] = property.Value.Clone();
                    }

                    var teachersInData = new HashSet<string>();
                    foreach (var day in _modifiedScheduleData.Values)
                    {
                        foreach (var lesson in day)
                        {
                            if (!string.IsNullOrEmpty(lesson.nauczyciel))
                                teachersInData.Add(lesson.nauczyciel);
                        }
                    }

                    foreach (var teacherName in teachersInData)
                    {
                        var existingSchedule = allSchedulesDict.ContainsKey(teacherName)
                            ? DeserializeScheduleFromElement(allSchedulesDict[teacherName])
                            : CreateEmptySchedule();

                        string currentClassName = GetCurrentClassName();
                        RemoveLessonsForClass(existingSchedule, currentClassName);
                        AddLessonsFromModifiedData(existingSchedule, teacherName, "teacher");

                        var scheduleJson = JsonSerializer.Serialize(existingSchedule);
                        var scheduleElement = JsonDocument.Parse(scheduleJson).RootElement.Clone();
                        allSchedulesDict[teacherName] = scheduleElement;

                        Console.WriteLine($"Zaktualizowano plan nauczyciela: {teacherName}");
                    }

                    SaveJsonFile(filePath, allSchedulesDict);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd UpdateTeacherSchedules: {ex.Message}");
                return false;
            }
        }

        private bool UpdateRoomSchedules(string plansPath)
        {
            try
            {
                string filePath = Path.Combine(plansPath, "room_schedules.json");
                if (!File.Exists(filePath)) return false;

                var jsonString = File.ReadAllText(filePath);
                using (JsonDocument document = JsonDocument.Parse(jsonString))
                {
                    var allSchedulesDict = new Dictionary<string, JsonElement>();
                    
                    foreach (JsonProperty property in document.RootElement.EnumerateObject())
                    {
                        allSchedulesDict[property.Name] = property.Value.Clone();
                    }

                    var roomsInData = new HashSet<string>();
                    foreach (var day in _modifiedScheduleData.Values)
                    {
                        foreach (var lesson in day)
                        {
                            if (!string.IsNullOrEmpty(lesson.sala))
                                roomsInData.Add(lesson.sala);
                        }
                    }

                    foreach (var roomName in roomsInData)
                    {
                        var existingSchedule = allSchedulesDict.ContainsKey(roomName)
                            ? DeserializeScheduleFromElement(allSchedulesDict[roomName])
                            : CreateEmptySchedule();

                        string currentClassName = GetCurrentClassName();
                        RemoveLessonsForClass(existingSchedule, currentClassName);
                        AddLessonsFromModifiedData(existingSchedule, roomName, "room");

                        var scheduleJson = JsonSerializer.Serialize(existingSchedule);
                        var scheduleElement = JsonDocument.Parse(scheduleJson).RootElement.Clone();
                        allSchedulesDict[roomName] = scheduleElement;

                        Console.WriteLine($"Zaktualizowano plan sali: {roomName}");
                    }

                    SaveJsonFile(filePath, allSchedulesDict);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd UpdateRoomSchedules: {ex.Message}");
                return false;
            }
        }

        // ========================================
        // METODY UPDATE - Dla planu nauczyciela/sali (bezpośrednia aktualizacja)
        // ========================================

        private bool UpdateTeacherSchedulesDirect(string plansPath)
        {
            try
            {
                string filePath = Path.Combine(plansPath, "teacher_schedules.json");
                if (!File.Exists(filePath)) return false;

                string teacherName = PlanTitle.Split(':')[1].Trim();

                var jsonString = File.ReadAllText(filePath);
                using (JsonDocument document = JsonDocument.Parse(jsonString))
                {
                    var allSchedulesDict = new Dictionary<string, JsonElement>();
                    
                    foreach (JsonProperty property in document.RootElement.EnumerateObject())
                    {
                        allSchedulesDict[property.Name] = property.Value.Clone();
                    }

                    var teacherSchedule = BuildTeacherScheduleDirect(teacherName);
                    var scheduleJson = JsonSerializer.Serialize(teacherSchedule);
                    var scheduleElement = JsonDocument.Parse(scheduleJson).RootElement.Clone();
                    allSchedulesDict[teacherName] = scheduleElement;

                    Console.WriteLine($"Zaktualizowano plan nauczyciela: {teacherName}");

                    SaveJsonFile(filePath, allSchedulesDict);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd UpdateTeacherSchedulesDirect: {ex.Message}");
                return false;
            }
        }

        private bool UpdateRoomSchedulesDirect(string plansPath)
        {
            try
            {
                string filePath = Path.Combine(plansPath, "room_schedules.json");
                if (!File.Exists(filePath)) return false;

                string roomName = PlanTitle.Split(':')[1].Trim();

                var jsonString = File.ReadAllText(filePath);
                using (JsonDocument document = JsonDocument.Parse(jsonString))
                {
                    var allSchedulesDict = new Dictionary<string, JsonElement>();
                    
                    foreach (JsonProperty property in document.RootElement.EnumerateObject())
                    {
                        allSchedulesDict[property.Name] = property.Value.Clone();
                    }

                    var roomSchedule = BuildRoomScheduleDirect(roomName);
                    var scheduleJson = JsonSerializer.Serialize(roomSchedule);
                    var scheduleElement = JsonDocument.Parse(scheduleJson).RootElement.Clone();
                    allSchedulesDict[roomName] = scheduleElement;

                    Console.WriteLine($"Zaktualizowano plan sali: {roomName}");

                    SaveJsonFile(filePath, allSchedulesDict);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd UpdateRoomSchedulesDirect: {ex.Message}");
                return false;
            }
        }

        private List<List<List<Dictionary<string, string>>>> BuildTeacherScheduleDirect(string teacherName)
        {
            var days = new[] { "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek", "Sobota" };
            var hours = LoadHourPlan();
            var result = new List<List<List<Dictionary<string, string>>>>();

            foreach (var day in days)
            {
                var daySchedule = new List<List<Dictionary<string, string>>>();

                foreach (var hour in hours)
                {
                    var hourLessons = new List<Dictionary<string, string>>();

                    if (_modifiedScheduleData.ContainsKey(day))
                    {
                        var lessons = _modifiedScheduleData[day]
                            .Where(l => l.godzina == hour && l.nauczyciel == teacherName)
                            .ToList();

                        foreach (var lesson in lessons)
                        {
                            hourLessons.Add(new Dictionary<string, string>
                            {
                                ["subject"] = lesson.przedmiot ?? "",
                                ["class"] = lesson.klasa ?? "",
                                ["room"] = lesson.sala ?? ""
                            });
                        }
                    }

                    daySchedule.Add(hourLessons);
                }

                result.Add(daySchedule);
            }

            return result;
        }

        private List<List<List<Dictionary<string, string>>>> BuildRoomScheduleDirect(string roomName)
        {
            var days = new[] { "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek", "Sobota" };
            var hours = LoadHourPlan();
            var result = new List<List<List<Dictionary<string, string>>>>();

            foreach (var day in days)
            {
                var daySchedule = new List<List<Dictionary<string, string>>>();

                foreach (var hour in hours)
                {
                    var hourLessons = new List<Dictionary<string, string>>();

                    if (_modifiedScheduleData.ContainsKey(day))
                    {
                        var lessons = _modifiedScheduleData[day]
                            .Where(l => l.godzina == hour && l.sala == roomName)
                            .ToList();

                        foreach (var lesson in lessons)
                        {
                            hourLessons.Add(new Dictionary<string, string>
                            {
                                ["subject"] = lesson.przedmiot ?? "",
                                ["teacher"] = lesson.nauczyciel ?? "",
                                ["class"] = lesson.klasa ?? ""
                            });
                        }
                    }

                    daySchedule.Add(hourLessons);
                }

                result.Add(daySchedule);
            }

            return result;
        }

        // HELPER METHODS

        private string GetCurrentClassName()
        {
            if (PlanTitle.Contains("Plan klasy:"))
            {
                return PlanTitle.Split(':')[1].Trim();
            }
            
            foreach (var day in _modifiedScheduleData.Values)
            {
                foreach (var lesson in day)
                {
                    if (!string.IsNullOrEmpty(lesson.klasa))
                        return lesson.klasa;
                }
            }
            
            return "";
        }

        private List<List<List<Dictionary<string, string>>>> CreateEmptySchedule()
        {
            var hours = LoadHourPlan();
            var result = new List<List<List<Dictionary<string, string>>>>();
            
            for (int d = 0; d < 6; d++)
            {
                var daySchedule = new List<List<Dictionary<string, string>>>();
                
                for (int h = 0; h < hours.Count; h++)
                {
                    daySchedule.Add(new List<Dictionary<string, string>>());
                }
                
                result.Add(daySchedule);
            }
            
            return result;
        }

        private List<List<List<Dictionary<string, string>>>> DeserializeScheduleFromElement(JsonElement element)
        {
            var result = new List<List<List<Dictionary<string, string>>>>();
            
            foreach (JsonElement day in element.EnumerateArray())
            {
                var daySchedule = new List<List<Dictionary<string, string>>>();
                
                foreach (JsonElement hour in day.EnumerateArray())
                {
                    var hourLessons = new List<Dictionary<string, string>>();
                    
                    foreach (JsonElement lesson in hour.EnumerateArray())
                    {
                        var lessonDict = new Dictionary<string, string>();
                        
                        foreach (JsonProperty prop in lesson.EnumerateObject())
                        {
                            lessonDict[prop.Name] = prop.Value.GetString() ?? "";
                        }
                        
                        hourLessons.Add(lessonDict);
                    }
                    
                    daySchedule.Add(hourLessons);
                }
                
                result.Add(daySchedule);
            }
            
            return result;
        }

        private void RemoveLessonsForClass(List<List<List<Dictionary<string, string>>>> schedule, string className)
        {
            foreach (var day in schedule)
            {
                foreach (var hour in day)
                {
                    hour.RemoveAll(lesson => lesson.ContainsKey("class") && lesson["class"] == className);
                }
            }
        }

        private void AddLessonsFromModifiedData(
            List<List<List<Dictionary<string, string>>>> schedule, 
            string filterValue, 
            string filterType)
        {
            var days = new[] { "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek", "Sobota" };
            var hours = LoadHourPlan();
            
            foreach (var dayKvp in _modifiedScheduleData)
            {
                int dayIndex = Array.IndexOf(days, dayKvp.Key);
                if (dayIndex < 0) continue;
                
                foreach (var lesson in dayKvp.Value)
                {
                    bool matches = false;
                    if (filterType == "teacher" && lesson.nauczyciel == filterValue)
                        matches = true;
                    else if (filterType == "room" && lesson.sala == filterValue)
                        matches = true;
                    
                    if (!matches) continue;
                    
                    int hourIndex = hours.IndexOf(lesson.godzina);
                    if (hourIndex < 0) continue;
                    
                    var lessonDict = new Dictionary<string, string>();
                    
                    if (filterType == "teacher")
                    {
                        lessonDict["subject"] = lesson.przedmiot ?? "";
                        lessonDict["class"] = lesson.klasa ?? "";
                        lessonDict["room"] = lesson.sala ?? "";
                    }
                    else if (filterType == "room")
                    {
                        lessonDict["subject"] = lesson.przedmiot ?? "";
                        lessonDict["teacher"] = lesson.nauczyciel ?? "";
                        lessonDict["class"] = lesson.klasa ?? "";
                    }
                    
                    schedule[dayIndex][hourIndex].Add(lessonDict);
                }
            }
        }

        private List<List<List<Dictionary<string, string>>>> BuildClassSchedule(string className)
        {
            var days = new[] { "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek", "Sobota" };
            var hours = LoadHourPlan();
            var result = new List<List<List<Dictionary<string, string>>>>();

            foreach (var day in days)
            {
                var daySchedule = new List<List<Dictionary<string, string>>>();

                foreach (var hour in hours)
                {
                    var hourLessons = new List<Dictionary<string, string>>();

                    if (_modifiedScheduleData.ContainsKey(day))
                    {
                        var lessons = _modifiedScheduleData[day]
                            .Where(l => l.godzina == hour && l.klasa == className)
                            .ToList();

                        foreach (var lesson in lessons)
                        {
                            hourLessons.Add(new Dictionary<string, string>
                            {
                                ["subject"] = lesson.przedmiot ?? "",
                                ["teacher"] = lesson.nauczyciel ?? "",
                                ["room"] = lesson.sala ?? ""
                            });
                        }
                    }

                    daySchedule.Add(hourLessons);
                }

                result.Add(daySchedule);
            }

            return result;
        }

        private void SaveJsonFile(string filePath, Dictionary<string, JsonElement> data)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
                {
                    writer.WriteStartObject();

                    foreach (var kvp in data)
                    {
                        writer.WritePropertyName(kvp.Key);
                        kvp.Value.WriteTo(writer);
                    }

                    writer.WriteEndObject();
                }

                var finalJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                File.WriteAllText(filePath, finalJson);
            }
        }

        private void MarkAllConflicts()
        {
            foreach (var hourRow in Hours)
            {
                foreach (var lesson in hourRow.Monday) lesson.HasConflict = false;
                foreach (var lesson in hourRow.Tuesday) lesson.HasConflict = false;
                foreach (var lesson in hourRow.Wednesday) lesson.HasConflict = false;
                foreach (var lesson in hourRow.Thursday) lesson.HasConflict = false;
                foreach (var lesson in hourRow.Friday) lesson.HasConflict = false;
            }

            foreach (var hourRow in Hours)
            {
                CheckAndMarkConflictsForHour(hourRow.Monday, hourRow.Hour);
                CheckAndMarkConflictsForHour(hourRow.Tuesday, hourRow.Hour);
                CheckAndMarkConflictsForHour(hourRow.Wednesday, hourRow.Hour);
                CheckAndMarkConflictsForHour(hourRow.Thursday, hourRow.Hour);
                CheckAndMarkConflictsForHour(hourRow.Friday, hourRow.Hour);
            }
        }

        private void CheckAndMarkConflictsForHour(ObservableCollection<PlansModel> lessons, string hour)
        {
            foreach (var lesson in lessons)
            {
                var conflicts = CheckConflicts(lesson, lesson.dzien, hour);
                lesson.HasConflict = conflicts.Count > 0;
            }
        }

        private class MissingSubjectInfo
        {
            public string SubjectName { get; set; }
            public int ExpectedHours { get; set; }
            public int CurrentHours { get; set; }
            public int MissingHours { get; set; }
        }
    }

    public class HourRow
    {
        public string Hour { get; set; }
        public ObservableCollection<PlansModel> Monday { get; set; } = new ObservableCollection<PlansModel>();
        public ObservableCollection<PlansModel> Tuesday { get; set; } = new ObservableCollection<PlansModel>();
        public ObservableCollection<PlansModel> Wednesday { get; set; } = new ObservableCollection<PlansModel>();
        public ObservableCollection<PlansModel> Thursday { get; set; } = new ObservableCollection<PlansModel>();
        public ObservableCollection<PlansModel> Friday { get; set; } = new ObservableCollection<PlansModel>();
    }
}