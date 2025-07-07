using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using Frontend.Models;

namespace Frontend.ViewModels
{
    public class ClassesViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ClassModel> _classes = new ObservableCollection<ClassModel>();
        public ObservableCollection<ClassModel> Classes
        {
            get => _classes;
            set
            {
                _classes = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddClassCommand { get; }
        public ICommand RemoveClassCommand { get; }
        public ICommand SaveCommand { get; }

        public ClassesViewModel()
        {
            LoadClasses();

            AddClassCommand = new RelayCommand(_ => AddClass());
            RemoveClassCommand = new RelayCommand<ClassModel>(RemoveClass);
            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
        }

        private void AddClass()
        {
            Classes.Add(new ClassModel
            {
                NazwaTypu = "",
                Ilosc_klas_typu = 0
            });
        }

        private void RemoveClass(ClassModel classModel)
        {
            if (classModel != null)
                Classes.Remove(classModel);
        }

        private void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(Classes);
                File.WriteAllText("class.json", json);
            }
            catch (IOException)
            {
                // Obsługa błędu zapisu
            }
        }

        private bool CanSave()
        {
            return true;
        }

        private void LoadClasses()
        {
            if (File.Exists("class.json"))
            {
                try
                {
                    var json = File.ReadAllText("class.json");
                    var loaded = JsonSerializer.Deserialize<ObservableCollection<ClassModel>>(json);
                    if (loaded != null)
                        Classes = loaded;
                }
                catch
                {
                    Classes = new ObservableCollection<ClassModel>();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
