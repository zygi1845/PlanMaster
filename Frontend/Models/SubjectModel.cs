using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Frontend.Models
{
    public class SubjectModel : INotifyPropertyChanged
    {
        private string name;
        private int hours;
        private string subjectType;
        private List<string> classTypes = new List<string>();

        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        public int Hours
        {
            get => hours;
            set { hours = value; OnPropertyChanged(); }
        }

        public string SubjectType
        {
            get => subjectType;
            set { subjectType = value; OnPropertyChanged(); }
        }

        public List<string> ClassTypes
        {
            get => classTypes;
            set { classTypes = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public override bool Equals(object obj)
        {
            if (obj is not SubjectModel other) return false;
            return Name == other.Name &&
                   Hours == other.Hours &&
                   SubjectType == other.SubjectType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Hours, SubjectType);
        }
    }
}
// using System.ComponentModel;
// using System.Runtime.CompilerServices;
// using System.Linq;
// using System;

// namespace Frontend.Models
// {
//     public class SubjectModel : INotifyPropertyChanged
//     {
//         private string name;
//         private int hours;
//         private string subjectType;

//         public string Name
//         {
//             get => name;
//             set { name = value; OnPropertyChanged(); }
//         }

//         public int Hours
//         {
//             get => hours;
//             set { hours = value; OnPropertyChanged(); }
//         }

//         public string SubjectType
//         {
//             get => subjectType;
//             set { subjectType = value; OnPropertyChanged(); }
//         }

//         public event PropertyChangedEventHandler PropertyChanged;
//         private void OnPropertyChanged([CallerMemberName] string name = null)
//             => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


//         public override bool Equals(object obj)
//         {
//             if (obj is not SubjectModel other) return false;
//             return Name == other.Name &&
//                    Hours == other.Hours &&
//                    SubjectType == other.SubjectType;
//         }

//         public override int GetHashCode()
//         {
//             return HashCode.Combine(Name, Hours, SubjectType);
//         }

//     }
// }
