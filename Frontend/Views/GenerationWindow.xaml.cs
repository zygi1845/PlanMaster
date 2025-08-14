using System.Windows;
using System.Windows.Input;

namespace Frontend.Views
{
    public partial class GenerationWindow : Window
    {
        public GenerationWindow()
        {
            InitializeComponent();
        }
                private void GenerateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is ViewModels.GenerationViewModel vm)
            {
                vm.GenerateTimetable();
            }
        }

        private void NavigateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is ViewModels.GenerationViewModel vm)
            {
                vm.Navigate(e.Parameter?.ToString());
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