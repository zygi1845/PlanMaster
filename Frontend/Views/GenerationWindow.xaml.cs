using System.Windows;
using INZYNIERKA.Shared;

namespace Frontend.Views
{
    public partial class GenerationWindow : Window
    {
        public GenerationWindow()
        {
            InitializeComponent(); // ← to zostanie wygenerowane automatycznie
            
            ClassCountTextBlock.Text = $"Ilość klas: {GlobalData.ClassCount}";
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
