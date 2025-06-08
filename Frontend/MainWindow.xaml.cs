using System.Windows;
using Frontend.Views;

namespace Frontend
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent(); // ← to zostanie wygenerowane automatycznie
        }

        private void HourPlanWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new Frontend.Views.HourPlanWindow();
            window.Show();
            this.Close();
        }
    }
}
