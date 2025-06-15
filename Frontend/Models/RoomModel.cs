using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Frontend.Models
{
    public class RoomModel : INotifyPropertyChanged
    {
        private string roomNumber;
        private string roomType;

        public string RoomNumber
        {
            get => roomNumber;
            set { roomNumber = value; OnPropertyChanged(); }
        }

        public string RoomType
        {
            get => roomType;
            set { roomType = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}