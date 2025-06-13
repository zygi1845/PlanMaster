using System.Windows;
using System.Windows.Controls;

namespace Frontend.Views
{
    public partial class TeachersWindow : Window
    {
        private Border teacherToDelete;
        public TeachersWindow()
        {
            InitializeComponent(); // ← to zostanie wygenerowane automatycznie
        }
        private void AddTeacher_Click(object sender, RoutedEventArgs e)
        {
            var teacherBorder = new Border
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
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var tb1 = new TextBox
            {
                Text = "Nr_pracownika",
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
                Text = "Nazwisko",
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219)),
                Width = 100,
                Height = 30,
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            var tb3 = new TextBox
            {
                Text = "Ustaw dostępność",
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
                Content = "Usuń pracownika",
                Background = System.Windows.Media.Brushes.Red,
                Width = 100,
                Height = 30,
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            deleteButton.Click += DeleteTeacher_Click;

            Grid.SetColumn(tb1, 0);
            Grid.SetColumn(tb2, 1);
            Grid.SetColumn(tb3, 2);
            Grid.SetColumn(deleteButton, 3);

            grid.Children.Add(tb1);
            grid.Children.Add(tb2);
            grid.Children.Add(tb3);
            grid.Children.Add(deleteButton);

            teacherBorder.Child = grid;
            TeachersPanel.Children.Add(teacherBorder);
        }

        private void DeleteTeacher_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Parent is Grid grid && grid.Parent is Border border)
            {
                teacherToDelete = border;
                DeleteConfirmation.Visibility = Visibility.Visible;
            }
        }

        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            if (teacherToDelete != null)
            {
                TeachersPanel.Children.Remove(teacherToDelete);
                teacherToDelete = null;
            }
            DeleteConfirmation.Visibility = Visibility.Collapsed;
        }

        private void CancelDelete_Click(object sender, RoutedEventArgs e)
        {
            teacherToDelete = null;
            DeleteConfirmation.Visibility = Visibility.Collapsed;
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
