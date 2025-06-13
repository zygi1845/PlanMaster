using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Frontend.Views
{
    public partial class WeightsWindow : Window
    {
        private List<string> subjects = new List<string> { "Matematyka", "Fizyka", "Biologia" }; // <- pobierz z danych

        public WeightsWindow()
        {
            InitializeComponent();
            AddMaxHoursBlock();
            AddBreaksBlock();
            AddSubjectBlocks();
        }

        private void AddMaxHoursBlock()
        {
            var border = CreateStyledBorder();
            var grid = Create3ColumnGrid();

            var labelBorder = CreateLabelBorder("Maksymalna liczba godzin w dniu: 8");
            Grid.SetColumn(labelBorder, 0);

    var textbox1 = CreateStyledTextBox("Ilość");
    Grid.SetColumn(textbox1, 1);

    var textbox2 = CreateStyledTextBox("Waga 0-10");
    Grid.SetColumn(textbox2, 2);

    grid.Children.Add(labelBorder);
    grid.Children.Add(textbox1);
    grid.Children.Add(textbox2);

    border.Child = grid;
    WeightsPanel.Children.Add(border);
}

private void AddBreaksBlock()
{
    var border = CreateStyledBorder();
    var grid = Create3ColumnGrid();

    var labelBorder = CreateLabelBorder("Przerwy między zajęciami");
    Grid.SetColumn(labelBorder, 0);

    var textbox = CreateStyledTextBox("Waga 0-10");
    Grid.SetColumn(textbox, 1);

    grid.Children.Add(labelBorder);
    grid.Children.Add(textbox);

    border.Child = grid;
    WeightsPanel.Children.Add(border);
}

private void AddSubjectBlocks()
{
    foreach (var subject in subjects)
    {
        var border = CreateStyledBorder();
        var grid = Create4ColumnGrid();

        var nameBorder = CreateLabelBorder($"Przedmiot: {subject}");
        Grid.SetColumn(nameBorder, 0);

        var hoursBox = CreateStyledTextBox("Ilość");
        Grid.SetColumn(hoursBox, 1);

        var maxPerDayBox = CreateStyledTextBox("Max/dzień");
        Grid.SetColumn(maxPerDayBox, 2);

        var weightBox = CreateStyledTextBox("Waga 0-10");
        Grid.SetColumn(weightBox, 3);

        grid.Children.Add(nameBorder);
        grid.Children.Add(hoursBox);
        grid.Children.Add(maxPerDayBox);
        grid.Children.Add(weightBox);

        border.Child = grid;
        WeightsPanel.Children.Add(border);
    }
}


private Border CreateLabelBorder(string text)
{
    return new Border
    {
        Background = new SolidColorBrush(Color.FromRgb(235, 245, 251)), // delikatny błękit
        BorderBrush = new SolidColorBrush(Color.FromRgb(52, 152, 219)), // niebieska ramka
        BorderThickness = new Thickness(1),
        Padding = new Thickness(5),
        Margin = new Thickness(0, 0, 10, 0),
        MaxWidth = 180,
        VerticalAlignment = VerticalAlignment.Center,
        Child = new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeights.SemiBold
        }
    };
}

        private Border CreateStyledBorder()
        {
            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(133, 193, 233)), // #85C1E9
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                Margin = new Thickness(10, 0, 0, 10),
                Width = 500,
                Height = 70,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };
        }

        private Grid Create3ColumnGrid()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            return grid;
        }

        private Grid Create4ColumnGrid()
        {
            var grid = new Grid();
            for (int i = 0; i < 4; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            return grid;
        }

        private TextBox CreateStyledTextBox(string placeholder)
        {
            return new TextBox
            {
                Text = placeholder,
                Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)), // #3498DB
                Width = 100,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
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
