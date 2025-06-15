using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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


    }
}
