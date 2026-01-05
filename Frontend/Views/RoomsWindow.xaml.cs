using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Frontend.ViewModels;

namespace Frontend.Views
{
    public partial class RoomsWindow : Window
    {
        private Border roomToDelete;

        public RoomsWindow()
        {
            InitializeComponent();
            DataContext = new RoomsViewModel();
        }

        // private void DeleteRoom_Click(object sender, RoutedEventArgs e)
        // {
        //     if (sender is Button button && button.Parent is Grid grid && grid.Parent is Border border)
        //     {
        //         roomToDelete = border;
        //         DeleteConfirmation.Visibility = Visibility.Visible;
        //         DeleteConfirmation.IsHitTestVisible = true;
        //     }
        // }

        // private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        // {
        //     if (roomToDelete != null)
        //     {
        //         Rooms.Children.Remove(roomToDelete);
        //         roomToDelete = null;
        //     }
        //     DeleteConfirmation.Visibility = Visibility.Collapsed;
        //     DeleteConfirmation.IsHitTestVisible = false;
        // }

        // private void CancelDelete_Click(object sender, RoutedEventArgs e)
        // {
        //     roomToDelete = null;
        //     DeleteConfirmation.Visibility = Visibility.Collapsed;
        //     DeleteConfirmation.IsHitTestVisible = false;
        // }

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
        private void PlansWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new PlansWindow();
            window.Show();
            this.Close();
        }
    }
}
