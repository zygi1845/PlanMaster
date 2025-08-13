using Frontend.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Frontend.ViewModels
{
    public class TeachersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<TeacherModel> _teachers = new ObservableCollection<TeacherModel>();
        public ObservableCollection<TeacherModel> Teachers
        {
            get => _teachers;
            set
            {
                _teachers = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddTeacherCommand { get; }
        public ICommand RemoveTeacherCommand { get; }
        public ICommand SaveCommand { get; }

        public TeachersViewModel()
        {
            LoadTeachers();

            AddTeacherCommand = new RelayCommand(_ => AddTeacher());
            RemoveTeacherCommand = new RelayCommand<TeacherModel>(RemoveTeacher);
            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
        }

        private void AddTeacher()
        {
            Teachers.Add(new TeacherModel
            {
                Nr_pracownika = 0,
                Nazwisko = "",
                Availability = new List<List<bool>>() // Inicjalizacja dostępności
            });
        }

        private void RemoveTeacher(TeacherModel teacher)
        {
            if (teacher != null)
                Teachers.Remove(teacher);
        }

        private void Save()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                var json = JsonSerializer.Serialize(Teachers, options);
                File.WriteAllText("teachers.json", json);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Błąd zapisu: {ex.Message}");
            }
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
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var json = File.ReadAllText("teachers.json");
                    var loaded = JsonSerializer.Deserialize<ObservableCollection<TeacherModel>>(json, options);
                    
                    if (loaded != null)
                    {
                        Teachers = loaded;
                        
                        // Inicjalizacja Availability jeśli null
                        foreach (var teacher in Teachers)
                        {
                            teacher.Availability ??= new List<List<bool>>();
                        }
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
}