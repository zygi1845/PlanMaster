using System.Windows;
using Frontend.ViewModels;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace Frontend.Views
{
    public partial class PlansWindow : Window
    {
        private PlansViewModel _viewModel;

        public PlansWindow()
        {
            InitializeComponent();
            _viewModel = new PlansViewModel();
            DataContext = _viewModel;
            
            // ✅ Ustaw licencję QuestPDF (darmowa dla projektów niekomercyjnych)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private void HourPlanWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new HourPlanWindow();
            window.Show();
            this.Close();
        }

        private void ClasesWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new ClasesWindow();
            window.Show();
            this.Close();
        }

        private void SubjectsWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new SubjectsWindow();
            window.Show();
            this.Close();
        }

        private void RoomsWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new RoomsWindow();
            window.Show();
            this.Close();
        }

        private void TeachersWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new TeachersWindow();
            window.Show();
            this.Close();
        }

        private void WeightsWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new WeightsWindow();
            window.Show();
            this.Close();
        }

        private void GenerationWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new GenerationWindow();
            window.Show();
            this.Close();
        }

        private void PlansWindow_Click(object sender, RoutedEventArgs e)
        {
            // Already on this window
        }

        // ✅ NOWA METODA: Eksport planów do osobnych PDF w folderach
        private void DownloadPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Wybór folderu docelowego (zamiast pojedynczego pliku)
                using (var folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "Wybierz folder gdzie zapisać plany";
                    folderDialog.ShowNewFolderButton = true;

                    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string basePath = folderDialog.SelectedPath;
                        
                        // Utwórz główny folder z datą
                        string mainFolder = Path.Combine(basePath, $"Plany_{DateTime.Now:yyyy-MM-dd_HH-mm}");
                        Directory.CreateDirectory(mainFolder);

                        // Generuj wszystkie plany
                        int totalGenerated = GenerateAllSchedulesPDF(mainFolder);

                        MessageBox.Show($"✅ Wygenerowano {totalGenerated} planów PDF!\n\n" +
                                      $"📁 Lokalizacja:\n{mainFolder}\n\n" +
                                      $"📂 Struktura:\n" +
                                      $"  📁 Klasy/\n" +
                                      $"  📁 Nauczyciele/\n" +
                                      $"  📁 Sale/",
                            "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Zapytaj czy otworzyć folder
                        var result = MessageBox.Show("Czy chcesz otworzyć folder z planami?",
                            "Otwórz folder", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = mainFolder,
                                UseShellExecute = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Błąd generowania PDF:\n\n{ex.Message}",
                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GenerateAllSchedulesPDF(string mainFolder)
        {
            int count = 0;

            // Wczytaj dane
            var classSchedules = LoadSchedulesFromJSON("class_schedules.json");
            var teacherSchedules = LoadSchedulesFromJSON("teacher_schedules.json");
            var roomSchedules = LoadSchedulesFromJSON("room_schedules.json");
            var hourPlan = LoadHourPlan();
            var days = new[] { "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek" };

            // 1. PLANY KLAS
            if (classSchedules.Count > 0)
            {
                string classesFolder = Path.Combine(mainFolder, "Klasy");
                Directory.CreateDirectory(classesFolder);

                foreach (var schedule in classSchedules)
                {
                    string fileName = SanitizeFileName($"{schedule.Key}.pdf");
                    string filePath = Path.Combine(classesFolder, fileName);
                    
                    GenerateSingleSchedulePDF(filePath, schedule.Key, schedule.Value, hourPlan, days, "Klasa");
                    count++;
                }
            }

            // 2. PLANY NAUCZYCIELI
            if (teacherSchedules.Count > 0)
            {
                string teachersFolder = Path.Combine(mainFolder, "Nauczyciele");
                Directory.CreateDirectory(teachersFolder);

                foreach (var schedule in teacherSchedules)
                {
                    string fileName = SanitizeFileName($"{schedule.Key}.pdf");
                    string filePath = Path.Combine(teachersFolder, fileName);
                    
                    GenerateSingleSchedulePDF(filePath, schedule.Key, schedule.Value, hourPlan, days, "Nauczyciel");
                    count++;
                }
            }

            // 3. PLANY SAL
            if (roomSchedules.Count > 0)
            {
                string roomsFolder = Path.Combine(mainFolder, "Sale");
                Directory.CreateDirectory(roomsFolder);

                foreach (var schedule in roomSchedules)
                {
                    string fileName = SanitizeFileName($"{schedule.Key}.pdf");
                    string filePath = Path.Combine(roomsFolder, fileName);
                    
                    GenerateSingleSchedulePDF(filePath, schedule.Key, schedule.Value, hourPlan, days, "Sala");
                    count++;
                }
            }

            return count;
        }

        private void GenerateSingleSchedulePDF(string outputPath, string name, 
            List<List<List<JsonElement>>> scheduleData, List<string> hourPlan, string[] days, string type)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Nagłówek
                    page.Header()
                        .Column(column =>
                        {
                            column.Item().Text($"Plan zajęć - {type}: {name}")
                                .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);
                            column.Item().Text($"Wygenerowano: {DateTime.Now:dd.MM.yyyy HH:mm}")
                                .FontSize(10).FontColor(Colors.Grey.Darken1);
                        });

                    // Tabela
                    page.Content()
                        .PaddingVertical(0.5f, Unit.Centimetre)
                        .Table(table =>
                        {
                            // Definiuj kolumny
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(70); // Godzina
                                foreach (var day in days)
                                {
                                    columns.RelativeColumn(); // Dni tygodnia
                                }
                            });

                            // Nagłówek tabeli
                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCellStyle).Text("Godzina").Bold().FontSize(11);
                                foreach (var day in days)
                                {
                                    header.Cell().Element(HeaderCellStyle).Text(day).Bold().FontSize(11);
                                }
                            });

                            // Wiersze z danymi
                            for (int hourIndex = 0; hourIndex < hourPlan.Count && hourIndex < scheduleData[0].Count; hourIndex++)
                            {
                                // Komórka z godziną
                                table.Cell().Element(TimeCellStyle).Text(hourPlan[hourIndex]).FontSize(9);

                                // Komórki z lekcjami dla każdego dnia
                                for (int dayIndex = 0; dayIndex < days.Length && dayIndex < scheduleData.Count; dayIndex++)
                                {
                                    var lessons = scheduleData[dayIndex][hourIndex];
                                    string cellContent = "";

                                    foreach (var lesson in lessons)
                                    {
                                        if (type == "Klasa")
                                        {
                                            string subject = GetJsonProperty(lesson, "subject");
                                            string teacher = GetJsonProperty(lesson, "teacher");
                                            string room = GetJsonProperty(lesson, "room");
                                            
                                            if (!string.IsNullOrEmpty(subject))
                                            {
                                                cellContent += $"{subject}\n";
                                                if (!string.IsNullOrEmpty(teacher))
                                                    cellContent += $"👨‍🏫 {teacher}\n";
                                                if (!string.IsNullOrEmpty(room))
                                                    cellContent += $"🚪 {room}\n";
                                            }
                                        }
                                        else if (type == "Nauczyciel")
                                        {
                                            string subject = GetJsonProperty(lesson, "subject");
                                            string className = GetJsonProperty(lesson, "class");
                                            string room = GetJsonProperty(lesson, "room");
                                            
                                            if (!string.IsNullOrEmpty(subject))
                                            {
                                                cellContent += $"{subject}\n";
                                                if (!string.IsNullOrEmpty(className))
                                                    cellContent += $"🎓 {className}\n";
                                                if (!string.IsNullOrEmpty(room))
                                                    cellContent += $"🚪 {room}\n";
                                            }
                                        }
                                        else if (type == "Sala")
                                        {
                                            string subject = GetJsonProperty(lesson, "subject");
                                            string teacher = GetJsonProperty(lesson, "teacher");
                                            string className = GetJsonProperty(lesson, "class");
                                            
                                            if (!string.IsNullOrEmpty(subject))
                                            {
                                                cellContent += $"{subject}\n";
                                                if (!string.IsNullOrEmpty(teacher))
                                                    cellContent += $"👨‍🏫 {teacher}\n";
                                                if (!string.IsNullOrEmpty(className))
                                                    cellContent += $"🎓 {className}\n";
                                            }
                                        }
                                    }

                                    table.Cell().Element(DataCellStyle).Text(cellContent.Trim()).FontSize(9);
                                }
                            }
                        });

                    // Stopka
                    page.Footer()
                        .AlignCenter()
                        .Text("Wygenerowano automatycznie przez system PlanMaster")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });
            })
            .GeneratePdf(outputPath);
        }

        // Style komórek
        private static IContainer HeaderCellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Blue.Darken2)
                .Background(Colors.Blue.Lighten3)
                .Padding(8)
                .AlignCenter()
                .AlignMiddle();
        }

        private static IContainer TimeCellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten1)
                .Background(Colors.Grey.Lighten3)
                .Padding(8)
                .AlignCenter()
                .AlignMiddle();
        }

        private static IContainer DataCellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten1)
                .Background(Colors.White)
                .Padding(8)
                .AlignLeft()
                .AlignTop();
        }

        // Sanitize nazwy pliku (usuń niedozwolone znaki)
        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            return sanitized;
        }

        // Wczytywanie danych z JSON
        private Dictionary<string, List<List<List<JsonElement>>>> LoadSchedulesFromJSON(string fileName)
        {
            try
            {
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo dir = new DirectoryInfo(currentPath);

                while (dir != null && dir.Name != "Inzynierka")
                {
                    dir = dir.Parent;
                }

                if (dir == null) return new Dictionary<string, List<List<List<JsonElement>>>>();

                string plansPath = Path.Combine(dir.FullName, "Backend", "algotithm", "plany");
                string filePath = Path.Combine(plansPath, fileName);

                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<Dictionary<string, List<List<List<JsonElement>>>>>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd wczytywania {fileName}: {ex.Message}");
            }

            return new Dictionary<string, List<List<List<JsonElement>>>>();
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
                Console.WriteLine($"Błąd wczytywania hourplan: {ex.Message}");
            }

            return new List<string> { "8:00-8:55", "9:00-9:55", "10:00-10:55", "11:00-11:55", "12:00-12:55", "13:00-13:55" };
        }

        private string GetJsonProperty(JsonElement element, string propertyName)
        {
            if (element.ValueKind == JsonValueKind.Object &&
                element.TryGetProperty(propertyName, out JsonElement value))
            {
                return value.GetString() ?? "";
            }
            return "";
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Czy na pewno chcesz zakończyć program?",
                "Zakończ", MessageBoxButton.YesNo, MessageBoxImage.Question);
    
            if (result == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}