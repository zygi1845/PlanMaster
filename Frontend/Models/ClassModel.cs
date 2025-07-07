using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Frontend.Models
{
    public class ClassModel : INotifyPropertyChanged
    {
        private string nazwaTypu;
        private int ilosc_klas_typu;

        public string NazwaTypu
        {
            get => nazwaTypu;
            set
            {
                nazwaTypu = value;
                OnPropertyChanged();
            }
        }

        public int Ilosc_klas_typu
        {
            get => ilosc_klas_typu;
            set
            {
                ilosc_klas_typu = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
