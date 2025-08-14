using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Frontend.Models
{
    public class TeacherModel : INotifyPropertyChanged
    {
        private int nr_pracownika;
        private string nazwisko;
        private SubjectModel _selectedSubject;
        private List<List<bool>> availability = new List<List<bool>>();

        public int Nr_pracownika
        {
            get => nr_pracownika;
            set { nr_pracownika = value; OnPropertyChanged(); }
        }

        public string Nazwisko
        {
            get => nazwisko;
            set { nazwisko = value; OnPropertyChanged(); }
        }

        [JsonInclude]
        public List<List<bool>> Availability
        {
            get => availability;
            set
            {
                availability = value;
                OnPropertyChanged();
            }
        }

        public SubjectModel SelectedSubject
        {
            get => _selectedSubject;
            set
            {
                _selectedSubject = value;
                OnPropertyChanged();
            }
        }
        
        public bool[,] GetAvailability()
        {
            if (Availability.Count == 0)
                return new bool[0, 0];

            var result = new bool[Availability.Count, Availability[0].Count];
            for (int i = 0; i < Availability.Count; i++)
            {
                for (int j = 0; j < Availability[i].Count; j++)
                {
                    result[i, j] = Availability[i][j];
                }
            }
            return result;
        }

        public void SetAvailability(bool[,] data)
        {
            var newAvailability = new List<List<bool>>();
            for (int i = 0; i < data.GetLength(0); i++)
            {
                var row = new List<bool>();
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    row.Add(data[i, j]);
                }
                newAvailability.Add(row);
            }
            Availability = newAvailability;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}