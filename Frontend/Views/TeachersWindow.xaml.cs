using System.Collections.Generic;
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
                var days = new List<string> { "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek" };

                // Pobierz aktualną dostępność nauczyciela
                bool[,] currentAvailability = teacher.GetAvailability();

                // Jeśli brak danych, utwórz nową tablicę
                if (currentAvailability.GetLength(0) != hours.Count ||
                    currentAvailability.GetLength(1) != days.Count)
                {
                    currentAvailability = new bool[hours.Count, days.Count];
                    for (int i = 0; i < hours.Count; i++)
                        for (int j = 0; j < days.Count; j++)
                            currentAvailability[i, j] = false; // Domyślnie dostępny
                }

                // Otwórz okno
                var window = new AvailabilityWindow(currentAvailability, hours, days);

                if (window.ShowDialog() == true)
                {
                    // Zapisz zmiany
                    teacher.SetAvailability(window.AvailabilityData);
                    (DataContext as TeachersViewModel)?.SaveCommand.Execute(null);
                }
            }
        }


    }
}
