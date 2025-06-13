using System.Windows;
using System.Windows.Controls;

namespace Frontend.Views
{
    public partial class RoomsWindow : Window
    {
        private Border roomToDelete;

        public RoomsWindow()
        {
            InitializeComponent();
        }

        private void AddRoom_Click(object sender, RoutedEventArgs e)
        {
            var roomBorder = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(133, 193, 233)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                Margin = new Thickness(10, 0, 0, 10),
                Width = 500,
                Height = 70,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var tb1 = new TextBox
            {
                Text = "Numer",
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219)),
                Width = 100,
                Height = 30,
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            var tb2 = new TextBox
            {
                Text = "Typ sali",
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219)),
                Width = 100,
                Height = 30,
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            var deleteButton = new Button
            {
                Content = "Usuń Sale",
                Background = System.Windows.Media.Brushes.Red,
                Width = 100,
                Height = 30,
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            deleteButton.Click += DeleteRoom_Click;

            Grid.SetColumn(tb1, 0);
            Grid.SetColumn(tb2, 1);
            Grid.SetColumn(deleteButton, 2);

            grid.Children.Add(tb1);
            grid.Children.Add(tb2);
            grid.Children.Add(deleteButton);

            roomBorder.Child = grid;
            RoomsPanel.Children.Add(roomBorder);
        }

        private void DeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Parent is Grid grid && grid.Parent is Border border)
            {
                roomToDelete = border;
                DeleteConfirmation.Visibility = Visibility.Visible;
                DeleteConfirmation.IsHitTestVisible = true;
            }
        }

        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            if (roomToDelete != null)
            {
                RoomsPanel.Children.Remove(roomToDelete);
                roomToDelete = null;
            }
            DeleteConfirmation.Visibility = Visibility.Collapsed;
            DeleteConfirmation.IsHitTestVisible = false;
        }

        private void CancelDelete_Click(object sender, RoutedEventArgs e)
        {
            roomToDelete = null;
            DeleteConfirmation.Visibility = Visibility.Collapsed;
            DeleteConfirmation.IsHitTestVisible = false;
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
