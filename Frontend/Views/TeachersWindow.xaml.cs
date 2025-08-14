/*using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Frontend.Models;
using Frontend.ViewModels;

namespace Frontend.Views
{
    public partial class TeachersWindow : Window
    {
        public TeachersWindow()
        {
            InitializeComponent();
            DataContext = new TeachersViewModel(); // ← to zostanie wygenerowane automatycznie
        }

        private void HourPlanWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new Frontend.Views.HourPlanWindow();
            window.Show();
            this.Close();
        }
        private void ClasesWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new Frontend.Views.ClasesWindow();
            window.Show();
            this.Close();
        }
        private void SubjectsWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new Frontend.Views.SubjectsWindow();
            window.Show();
            this.Close();
        }
        private void RoomsWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new Frontend.Views.RoomsWindow();
            window.Show();
            this.Close();
        }
        private void TeachersWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new Frontend.Views.TeachersWindow();
            window.Show();
            this.Close();
        }
        private void WeightsWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new Frontend.Views.WeightsWindow();
            window.Show();
            this.Close();
        }
        private void GenerationWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new Frontend.Views.GenerationWindow();
            window.Show();
            this.Close();
        }
        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (string.IsNullOrEmpty(tb.Text))
            {
                tb.Text = tb.Tag.ToString();
                tb.Foreground = Brushes.DarkGray;
            }
        }


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.Text == tb.Tag.ToString())
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black;
            }
        }


        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = tb.Tag.ToString();
                tb.Foreground = Brushes.DarkGray;
            }
        }

        private void SetAvailability_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is TeacherModel teacher)
            {
                // Wczytaj godziny z planu
                var hourPlan = new HourPlanViewModel();
                var hours = hourPlan.Rows.Select(r => r.Hour).ToList();

                // Tutaj dodajemy pełne nazwy dni tygodnia
                var daysOfWeek = new List<string>
        {
            "Poniedziałek",
            "Wtorek",
            "Środa",
            "Czwartek",
            "Piątek"
        };

                bool[,] availability = teacher.GetAvailability();

                // Inicjalizacja jeśli brak danych
                if (availability.GetLength(0) != hours.Count ||
                    availability.GetLength(1) != daysOfWeek.Count)
                {
                    availability = new bool[hours.Count, daysOfWeek.Count];
                    for (int i = 0; i < hours.Count; i++)
                        for (int j = 0; j < daysOfWeek.Count; j++)
                            availability[i, j] = false; // Domyślnie niedostępne
                }

                var window = new AvailabilityWindow(availability, hours, daysOfWeek);
                if (window.ShowDialog() == true)
                {
                    teacher.SetAvailability(window.AvailabilityData);
                    (DataContext as TeachersViewModel)?.SaveCommand.Execute(null);
                }
            }
        }



    }
}
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Frontend.ViewModels;
using Frontend.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace Frontend.Views
{
    public partial class TeachersWindow : Window
    {
        public ObservableCollection<SubjectModel> SubjectsList { get; set; } = new ObservableCollection<SubjectModel>();

        public TeachersWindow()
        {
            InitializeComponent();
            LoadSubjects();
            DataContext = new TeachersViewModel();
        }

        private void LoadSubjects()
        {
            if (File.Exists("subjects.json"))
            {
                try
                {
                    var json = File.ReadAllText("subjects.json");
                    var loaded = JsonSerializer.Deserialize<ObservableCollection<SubjectModel>>(json);
                    SubjectsList = loaded ?? new ObservableCollection<SubjectModel>();
                }
                catch
                {
                    SubjectsList = new ObservableCollection<SubjectModel>();
                }
            }
            else
            {
                SubjectsList = new ObservableCollection<SubjectModel>();
            }
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                comboBox.ItemsSource = SubjectsList;
                comboBox.MaxDropDownHeight = 90; // 3 pozycje * 30px każda
            }
        }

        private void HourPlanWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new HourPlanWindow();
            window.Show();
            this.Close();
        }

        private void ClasesWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new ClasesWindow();
            window.Show();
            this.Close();
        }

        private void SubjectsWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new SubjectsWindow();
            window.Show();
            this.Close();
        }

        private void RoomsWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new RoomsWindow();
            window.Show();
            this.Close();
        }

        private void TeachersWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new TeachersWindow();
            window.Show();
            this.Close();
        }

        private void WeightsWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new WeightsWindow();
            window.Show();
            this.Close();
        }

        private void GenerationWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new GenerationWindow();
            window.Show();
            this.Close();
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (string.IsNullOrEmpty(tb.Text))
            {
                tb.Text = tb.Tag.ToString();
                tb.Foreground = Brushes.DarkGray;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.Text == tb.Tag.ToString())
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = tb.Tag.ToString();
                tb.Foreground = Brushes.DarkGray;
            }
        }

        private void SetAvailability_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is TeacherModel teacher)
            {
                // Wczytaj godziny z planu
                var hourPlan = new HourPlanViewModel();
                var hours = hourPlan.Rows.Select(r => r.Hour).ToList();

                // Tutaj dodajemy pełne nazwy dni tygodnia
                var daysOfWeek = new List<string>
        {
            "Poniedziałek",
            "Wtorek",
            "Środa",
            "Czwartek",
            "Piątek"
        };

                bool[,] availability = teacher.GetAvailability();

                // Inicjalizacja jeśli brak danych
                if (availability.GetLength(0) != hours.Count ||
                    availability.GetLength(1) != daysOfWeek.Count)
                {
                    availability = new bool[hours.Count, daysOfWeek.Count];
                    for (int i = 0; i < hours.Count; i++)
                        for (int j = 0; j < daysOfWeek.Count; j++)
                            availability[i, j] = false; // Domyślnie niedostępne
                }

                var window = new AvailabilityWindow(availability, hours, daysOfWeek);
                if (window.ShowDialog() == true)
                {
                    teacher.SetAvailability(window.AvailabilityData);
                    (DataContext as TeachersViewModel)?.SaveCommand.Execute(null);
                }
            }
        }


    
    }
}