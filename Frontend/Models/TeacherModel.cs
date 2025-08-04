using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Frontend.Models
{
    public class TeacherModel : INotifyPropertyChanged
    {
        private int nr_pracownika;
        private string nazwisko;

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

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string nazwisko = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nazwisko));

        
    private List<List<bool>> availability = new List<List<bool>>();
    
    public bool[,] GetAvailability()
    {
        if (availability.Count == 0) return new bool[0,0];
        
        var result = new bool[availability.Count, availability[0].Count];
        for (int i = 0; i < availability.Count; i++)
        {
            for (int j = 0; j < availability[i].Count; j++)
            {
                result[i, j] = availability[i][j];
            }
        }
        return result;
    }

    public void SetAvailability(bool[,] data)
    {
        availability.Clear();
        for (int i = 0; i < data.GetLength(0); i++)
        {
            var row = new List<bool>();
            for (int j = 0; j < data.GetLength(1); j++)
            {
                row.Add(data[i, j]);
            }
            availability.Add(row);
        }
    }
    }
}
