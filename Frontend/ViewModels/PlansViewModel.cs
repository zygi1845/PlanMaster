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
    public class PlansViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<PlanItem> _classes = new ObservableCollection<PlanItem>();
        private ObservableCollection<PlanItem> _teachers = new ObservableCollection<PlanItem>();
        private ObservableCollection<PlanItem> _rooms = new ObservableCollection<PlanItem>();
        private bool _isClassView = true;
        private bool _isTeacherView = false;
        private bool _isRoomView = false;

        public ObservableCollection<PlanItem> CurrentItems
        {
            get
            {
                if (IsClassView) return Classes;
                if (IsTeacherView) return Teachers;
                if (IsRoomView) return Rooms;
                return Classes;
            }
        }

        public ObservableCollection<PlanItem> Classes
        {
            get => _classes;
            set
            {
                _classes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentItems));
            }
        }

        public ObservableCollection<PlanItem> Teachers
        {
            get => _teachers;
            set
            {
                _teachers = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentItems));
            }
        }

        public ObservableCollection<PlanItem> Rooms
        {
            get => _rooms;
            set
            {
                _rooms = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentItems));
            }
        }

        public bool IsClassView
        {
            get => _isClassView;
            set
            {
                _isClassView = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentItems));
            }
        }

        public bool IsTeacherView
        {
            get => _isTeacherView;
            set
            {
                _isTeacherView = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentItems));
            }
        }

        public bool IsRoomView
        {
            get => _isRoomView;
            set
            {
                _isRoomView = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentItems));
            }
        }

        public ICommand ShowDetailsCommand { get; }

        public PlansViewModel()
        {
            LoadData();
            ShowDetailsCommand = new RelayCommand<PlanItem>(ShowDetails);
        }

        private void LoadData()
        {
            Console.WriteLine("Ładowanie danych...");
            LoadClasses();
            LoadTeachers();
            LoadRooms();
            Console.WriteLine($"Załadano: {Classes.Count} klas, {Teachers.Count} nauczycieli, {Rooms.Count} sal");
            
            // Tymczasowe dane testowe jeśli pliki nie istnieją
            if (Classes.Count == 0)
            {
                Classes.Add(new PlanItem { Name = "Testowa Klasa 1A", Type = "Class" });
                Classes.Add(new PlanItem { Name = "Testowa Klasa 2B", Type = "Class" });
                Console.WriteLine("Dodano testowe klasy");
            }
            if (Teachers.Count == 0)
            {
                Teachers.Add(new PlanItem { Name = "Jan Kowalski", Type = "Teacher" });
                Teachers.Add(new PlanItem { Name = "Anna Nowak", Type = "Teacher" });
                Console.WriteLine("Dodano testowych nauczycieli");
            }
            if (Rooms.Count == 0)
            {
                Rooms.Add(new PlanItem { Name = "Sala 101", Type = "Room" });
                Rooms.Add(new PlanItem { Name = "Sala 202", Type = "Room" });
                Console.WriteLine("Dodano testowe sale");
            }
        }

        private void LoadClasses()
        {
            try
            {
                string filePath = @"C:\Users\Zygmunt\Desktop\Semestr 6\PH_Projekt\Inzynierka\Frontend\plany\plany_klas.json";
                Console.WriteLine($"Szukam pliku: {filePath}");
                Console.WriteLine($"File.Exists: {File.Exists(filePath)}");

                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    Console.WriteLine($"Plik plany_klas.json znaleziony, rozmiar: {json.Length} znaków");

                    var classPlans = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<PlansModel>>>>(json);

                    Classes.Clear();
                    foreach (var className in classPlans.Keys)
                    {
                        Console.WriteLine($"Dodano klasę: {className}");
                        Classes.Add(new PlanItem { Name = className, Type = "Class" });
                    }
                }
                else
                {
                    Console.WriteLine("PLIK NIE ISTNIEJE! Sprawdzam dostępne pliki w folderze:");
                    string directory = @"C:\Users\Zygmunt\Desktop\Semestr 6\PH_Projekt\Inzynierka\Frontend\plany\";
                    if (Directory.Exists(directory))
                    {
                        foreach (var file in Directory.GetFiles(directory))
                        {
                            Console.WriteLine($"Znaleziony plik: {file}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Folder nie istnieje!");
                    }
                }
        
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BŁĄD wczytywania klas: {ex.Message}");
                MessageBox.Show($"Błąd wczytywania klas: {ex.Message}");
            }
        }


        private void LoadTeachers()
        {
            try
            {
                string filePath = @"C:\Users\Zygmunt\Desktop\Semestr 6\PH_Projekt\Inzynierka\Frontend\plany\plany_nauczycieli.json";
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    Console.WriteLine($"Plik plany_nauczycieli.json znaleziony, rozmiar: {json.Length} znaków");

                    var teacherPlans = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<PlansModel>>>>(json);

                    Teachers.Clear();
                    foreach (var teacherName in teacherPlans.Keys)
                    {
                        Console.WriteLine($"Dodano nauczyciela: {teacherName}");
                        Teachers.Add(new PlanItem { Name = teacherName, Type = "Teacher" });
                    }
                }
                else
                {
                    Console.WriteLine($"Plik {filePath} nie istnieje!");
                    string absolutePath = Path.GetFullPath(filePath);
                    Console.WriteLine($"Absolutna ścieżka: {absolutePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BŁĄD wczytywania nauczycieli: {ex.Message}");
                MessageBox.Show($"Błąd wczytywania nauczycieli: {ex.Message}");
            }
        }


        private void LoadRooms()
        {
            try
            {
                string filePath = @"C:\Users\Zygmunt\Desktop\Semestr 6\PH_Projekt\Inzynierka\Frontend\plany\plany_sal.json";
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    Console.WriteLine($"Plik plany_sal.json znaleziony, rozmiar: {json.Length} znaków");

                    var roomPlans = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<PlansModel>>>>(json);

                    Rooms.Clear();
                    foreach (var roomName in roomPlans.Keys)
                    {
                        Console.WriteLine($"Dodano salę: {roomName}");
                        Rooms.Add(new PlanItem { Name = roomName, Type = "Room" });
                    }
                }
                else
                {
                    Console.WriteLine($"Plik {filePath} nie istnieje!");
                    string absolutePath = Path.GetFullPath(filePath);
                    Console.WriteLine($"Absolutna ścieżka: {absolutePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BŁĄD wczytywania sal: {ex.Message}");
                MessageBox.Show($"Błąd wczytywania sal: {ex.Message}");
            }
        }


        private void ShowDetails(PlanItem item)
        {
            if (item == null) return;

            try
            {
                string fileName = "";
                string filePath = "";
                string title = "";

                switch (item.Type)
                {
                    case "Class":
                        fileName = "plany_klas.json";
                        filePath = @"C:\Users\Zygmunt\Desktop\Semestr 6\PH_Projekt\Inzynierka\Frontend\plany\plany_klas.json";
                        title = $"Plan klasy: {item.Name}";
                        break;
                    case "Teacher":
                        fileName = "plany_nauczycieli.json";
                        filePath = @"C:\Users\Zygmunt\Desktop\Semestr 6\PH_Projekt\Inzynierka\Frontend\plany\plany_nauczycieli.json";
                        title = $"Plan nauczyciela: {item.Name}";
                        break;
                    case "Room":
                        fileName = "plany_sal.json";
                        filePath = @"C:\Users\Zygmunt\Desktop\Semestr 6\PH_Projekt\Inzynierka\Frontend\plany\plany_sal.json";
                        title = $"Plan sali: {item.Name}";
                        break;
                }

                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var plans = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<PlansModel>>>>(json);

                    if (plans.ContainsKey(item.Name))
                    {
                        var detailWindow = new PlanDetailWindow(item.Name, title, plans[item.Name]);
                        detailWindow.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show($"Nie znaleziono planu dla: {item.Name}");
                    }
                }
                else
                {
                    MessageBox.Show($"Plik {filePath} nie istnieje!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd wczytywania szczegółów: {ex.Message}");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class PlanItem
    {
        public string Name { get; set; }
        public string Type { get; set; } // "Class", "Teacher", "Room"
    }
}