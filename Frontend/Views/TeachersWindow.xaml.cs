using System.Windows;
using System.Windows.Controls;
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
        public TeachersWindow()
        {
            InitializeComponent();
            DataContext = new TeachersViewModel();
        }

        private void SetAvailability_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is TeacherModel teacher)
            {
                // Wczytaj godziny z planu
                var hourPlan = new HourPlanViewModel();
                var hours = hourPlan.Rows.Select(r => r.Hour).ToList();

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
                            availability[i, j] = false;
                }

                var window = new AvailabilityWindow(availability, hours, daysOfWeek);
                if (window.ShowDialog() == true)
                {
                    teacher.SetAvailability(window.AvailabilityData);
                }
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
    }
}