using Frontend.Models;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;
using System;

namespace Frontend.ViewModels
{
    public class GenerationViewModel : INotifyPropertyChanged
    {
        private readonly GenerationModel _model = new GenerationModel();
        private bool _isGenerating = false;

        public int TeacherCount => _model.TeacherCount;
        public int RoomCount => _model.RoomCount;
        public int ClassCount => _model.ClassCount;
        public int ClassTypeCount => _model.ClassTypeCount;

        public bool IsGenerating
        {
            get => _isGenerating;
            set
            {
                _isGenerating = value;
                OnPropertyChanged();
            }
        }

        // Statyczne komendy RoutedUICommand
        public static RoutedUICommand GenerateCommand { get; } = 
            new RoutedUICommand("Generuj plan", "GenerateCommand", typeof(GenerationViewModel));
        
        public static RoutedUICommand NavigateCommand { get; } = 
            new RoutedUICommand("Nawiguj", "NavigateCommand", typeof(GenerationViewModel));

        public GenerationViewModel()
        {
            _model.LoadData();
        }

        public void GenerateTimetable(Action<bool, string> callback)
        {
            IsGenerating = true;

            try
            {
                // Znajdź katalog Inzynierka
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo dir = new DirectoryInfo(currentPath);
                
                while (dir != null && dir.Name != "Inzynierka")
                {
                    dir = dir.Parent;
                }

                if (dir == null)
                {
                    callback(false, "Nie znaleziono katalogu Inzynierka!");
                    IsGenerating = false;
                    return;
                }

                string pythonScriptPath = Path.Combine(dir.FullName, "Backend", "algotithm", "main.py");
                string workingDirectory = Path.Combine(dir.FullName, "Backend", "algotithm");

                if (!File.Exists(pythonScriptPath))
                {
                    callback(false, $"Nie znaleziono pliku main.py w:\n{pythonScriptPath}");
                    IsGenerating = false;
                    return;
                }

                // Uruchom Python
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{pythonScriptPath}\"",
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            Console.WriteLine($"Python: {e.Data}");
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            Console.WriteLine($"Python ERROR: {e.Data}");
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Czekaj maksymalnie 5 minut
                    bool finished = process.WaitForExit(300000); // 5 min timeout

                    if (finished)
                    {
                        // Sprawdź czy pliki zostały utworzone w Backend/algotithm/plany
                        string plansPath = Path.Combine(workingDirectory, "plany");
                        string file1 = Path.Combine(plansPath, "class_schedules.json");
                        string file2 = Path.Combine(plansPath, "teacher_schedules.json");
                        string file3 = Path.Combine(plansPath, "room_schedules.json");

                        Console.WriteLine($"Sprawdzam pliki:");
                        Console.WriteLine($"  - {file1} : {File.Exists(file1)}");
                        Console.WriteLine($"  - {file2} : {File.Exists(file2)}");
                        Console.WriteLine($"  - {file3} : {File.Exists(file3)}");
                        
                        bool allFilesExist = File.Exists(file1) && File.Exists(file2) && File.Exists(file3);

                        if (allFilesExist)
                        {
                            callback(true, "Plan wygenerowany pomyślnie!");
                        }
                        else
                        {
                            callback(false, $"Algorytm zakończył się, ale nie utworzył wszystkich plików.\nSzukano w: {plansPath}");
                        }
                    }
                    else
                    {
                        process.Kill();
                        callback(false, "Generowanie przekroczyło limit czasu (5 minut).");
                    }
                }
            }
            catch (Exception ex)
            {
                callback(false, $"Błąd uruchamiania algorytmu:\n{ex.Message}\n\nUpewnij się, że Python jest zainstalowany.");
            }
            finally
            {
                IsGenerating = false;
            }
        }

        public void Navigate(string destination)
        {
            MessageBox.Show($"Nawigacja do: {destination}");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
// using Frontend.Models;
// using System.Windows;
// using System.ComponentModel;
// using System.Runtime.CompilerServices;
// using System.Windows.Input;

// namespace Frontend.ViewModels
// {
//     public class GenerationViewModel : INotifyPropertyChanged
//     {
//         private readonly GenerationModel _model = new GenerationModel();

//         public int TeacherCount => _model.TeacherCount;
//         public int RoomCount => _model.RoomCount;
//         public int ClassCount => _model.ClassCount;
//         public int ClassTypeCount => _model.ClassTypeCount;

//         // Statyczne komendy RoutedUICommand
//         public static RoutedUICommand GenerateCommand { get; } = 
//             new RoutedUICommand("Generuj plan", "GenerateCommand", typeof(GenerationViewModel));
        
//         public static RoutedUICommand NavigateCommand { get; } = 
//             new RoutedUICommand("Nawiguj", "NavigateCommand", typeof(GenerationViewModel));

//         public GenerationViewModel()
//         {
//             _model.LoadData();
//         }

//         public void GenerateTimetable()
//         {
//             MessageBox.Show($"Generowanie planu:\nNauczyciele: {TeacherCount}\nSale: {RoomCount}\nKlasy: {ClassCount}\nTypy klas: {ClassTypeCount}");
//         }

//         public void Navigate(string destination)
//         {
//             MessageBox.Show($"Nawigacja do: {destination}");
//             // Tutaj implementacja zmiany okien
//         }

//         public event PropertyChangedEventHandler PropertyChanged;
//         protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
//         {
//             PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//         }
//     }
// }