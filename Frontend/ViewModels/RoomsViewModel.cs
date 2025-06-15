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

namespace Frontend.ViewModels
{
    public class RoomsViewModel : INotifyPropertyChanged
    {

        // Lista typów zajęć do bindowania w ComboBox
        public List<string> RoomsTypes { get; } = new List<string>
        {
            "Pracownia Specjalistyczna",
            "Wykłady",
            "Ćwiczenia"
        };

        private ObservableCollection<RoomModel> _rooms = new ObservableCollection<RoomModel>();
        public ObservableCollection<RoomModel> Rooms
        {
            get => _rooms;
            set
            {
                _rooms = value;
                OnPropertyChanged();
            }
        }
        public ICommand AddRoomCommand { get; }
        public ICommand RemoveRoomCommand { get; }
        public ICommand SaveCommand { get; }

        public RoomsViewModel()
        {
            LoadRooms();

            AddRoomCommand = new RelayCommand(_ => AddRoom());
            RemoveRoomCommand = new RelayCommand<RoomModel>(RemoveRoom);
            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
        }

        private void AddRoom()
        {
            Rooms.Add(new RoomModel
            {
                RoomNumber = "",
                RoomType = RoomsTypes[0] // domyślny typ
            });
        }

        private void RemoveRoom(RoomModel room)
        {
            if (room != null)
                Rooms.Remove(room);
        }

        private void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(Rooms);
                File.WriteAllText("rooms.json", json);
            }
            catch (IOException)
            {
                // Można dodać logowanie błędu lub powiadomienie użytkownika
            }
        }

        private bool CanSave()
        {
            return true;
        }

        private bool AreRoomNumbersUnique()
        {
            return Rooms.Select(r => r.RoomNumber).Distinct().Count() == Rooms.Count;
        }

        private void LoadRooms()
        {
            if (File.Exists("rooms.json"))
            {
                try
                {
                    var json = File.ReadAllText("rooms.json");
                    var loaded = JsonSerializer.Deserialize<ObservableCollection<RoomModel>>(json);
                    if (loaded != null)
                        Rooms = loaded;
                }
                catch
                {
                    Rooms = new ObservableCollection<RoomModel>();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}


// using Frontend.Models;
// using System.Collections.ObjectModel;
// using System.ComponentModel;
// using System.IO;
// using System.Linq;
// using System.Runtime.CompilerServices;
// using System.Text.Json;
// using System.Windows.Input;

// namespace Frontend.ViewModels
// {
//     public class RoomsViewModel : INotifyPropertyChanged
//     {
//         private const string SaveFile = "rooms.json";

//         public ObservableCollection<RoomModel> Rooms { get; set; } = new ObservableCollection<RoomModel>();
//         public ICommand AddRoomCommand { get; }
//         public ICommand RemoveRoomCommand { get; }
//         public ICommand SaveCommand { get; }

//         public RoomsViewModel()
//         {
//             Load();

//             AddRoomCommand = new RelayCommand(_ => AddRoom());
//             RemoveRoomCommand = new RelayCommand<RoomModel>(RemoveRoom);
//             SaveCommand = new RelayCommand(_ => Save());
//         }

//         private void AddRoom()
//         {
//             Rooms.Add(new RoomModel {
//                 RoomNumber = "",
//                 Type = RoomType.PracowniaSpecjalistyczna
//             });
//         }

//         private void RemoveRoom(RoomModel room)
//         {
//             if (room != null)
//                 Rooms.Remove(room);
//         }

//         public void Save()
//         {
//             var distinctNumbers = Rooms.Select(r => r.RoomNumber).Distinct().Count() == Rooms.Count;
//             if (!distinctNumbers) return; // nie zapisuj jeśli numer się powtarza

//             File.WriteAllText(SaveFile, JsonSerializer.Serialize(Rooms));
//         }

//         private void Load()
//         {
//             if (File.Exists(SaveFile))
//             {
//                 try
//                 {
//                     var loaded = JsonSerializer.Deserialize<ObservableCollection<RoomModel>>(File.ReadAllText(SaveFile));
//                     if (loaded != null)
//                         Rooms = loaded;
//                 }
//                 catch { Rooms = new ObservableCollection<RoomModel>(); }
//             }
//         }

//         public event PropertyChangedEventHandler PropertyChanged;
//         private void OnPropertyChanged([CallerMemberName] string n = null) => 
//             PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
//     }
// }
