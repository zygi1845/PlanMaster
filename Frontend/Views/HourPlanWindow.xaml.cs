using Frontend.Models;
using Frontend.ViewModels;
using System.Windows;

namespace Frontend.Views
{
    public partial class HourPlanWindow : Window
    {
        private HourPlanViewModel _viewModel;

        public HourPlanWindow()
        {
            InitializeComponent();
            _viewModel = new HourPlanViewModel();
            DataContext = _viewModel;
            MyDataGrid.ItemsSource = _viewModel.Rows;
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.AddRow();
        }

        private void DeleteRow_Click(object sender, RoutedEventArgs e)
        {
            if (MyDataGrid.SelectedItem is HourPlanRow selectedRow)
            {
                _viewModel.RemoveRow(selectedRow);
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveToFile();
            MessageBox.Show("Plan godzinowy został zapisany.");
        }

        private void HourPlanWindow_Click(object sender, RoutedEventArgs e)
        {
            // Already on this window
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

        // ✅ DODANO: Przycisk Plany
        private void PlansWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new PlansWindow();
            window.Show();
            this.Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Czy na pewno chcesz zakończyć program?",
                "Zakończ", MessageBoxButton.YesNo, MessageBoxImage.Question);
    
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

    }
}