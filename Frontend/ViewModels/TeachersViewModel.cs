using Frontend.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System;
using System.IO;
using System.Text.Json;
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
                Nazwisko = "",         // domyślny typ z listy
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
                var json = JsonSerializer.Serialize(Teachers);
                File.WriteAllText("teachers.json", json);
            }
            catch (IOException)
            {
                // Obsługa błędu zapisu
            }
        }

        private bool CanSave()
        {
            // Tu możesz dodać walidację, np. sprawdzić puste nazwy
            return true;
        }

        private void LoadTeachers()
        {
            if (File.Exists("teachers.json"))
            {
                try
                {
                    var json = File.ReadAllText("teachers.json");
                    var loaded = JsonSerializer.Deserialize<ObservableCollection<TeacherModel>>(json);
                    if (loaded != null)
                        Teachers = loaded;
                }
                catch
                {
                    Teachers = new ObservableCollection<TeacherModel>();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string nazwisko = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nazwisko));
    }
}

