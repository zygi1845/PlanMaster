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
    }
}
