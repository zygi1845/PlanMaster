using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Frontend.Views
{
    public partial class AvailabilityWindow : Window
    {
        private readonly bool[,] availability;
        private readonly List<string> hours;
        private readonly List<string> days;
        public bool[,] AvailabilityData => availability;

        public AvailabilityWindow(bool[,] existingAvailability, List<string> hours, List<string> days)
        {
            InitializeComponent();

            this.hours = hours;
            this.days = days;

            int rows = hours.Count;
            int cols = days.Count;

            // Jeśli przekazana tablica jest null lub rozmiar się nie zgadza, utwórz nową
            if (existingAvailability == null || existingAvailability.GetLength(0) != rows || existingAvailability.GetLength(1) != cols)
            {
                availability = new bool[rows, cols];
            }
            else
            {
                availability = existingAvailability;
            }

            GenerateGrid();
        }

        private void GenerateGrid()
        {
            AvailabilityGrid.RowDefinitions.Clear();
            AvailabilityGrid.ColumnDefinitions.Clear();
            AvailabilityGrid.Children.Clear();

            // Dodaj 1 wiersz więcej na nagłówki godzin
            AvailabilityGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Dodaj wiersze dla każdej godziny
            foreach (var _ in hours)
                AvailabilityGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });

            // Dodaj 1 kolumnę więcej na nagłówek dni
            AvailabilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Dodaj kolumny dla dni
            foreach (var _ in days)
                AvailabilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

            // Nagłówki dni (kolumny) - od kolumny 1 w górnym wierszu
            for (int c = 0; c < days.Count; c++)
            {
                var tb = new TextBlock
                {
                    Text = days[c],
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(3)
                };
                Grid.SetRow(tb, 0);
                Grid.SetColumn(tb, c + 1);
                AvailabilityGrid.Children.Add(tb);
            }

            // Nagłówki godzin (wiersze) - od wiersza 1 w pierwszej kolumnie
            for (int r = 0; r < hours.Count; r++)
            {
                var tb = new TextBlock
                {
                    Text = hours[r],
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(3,0,3,0)
                };
                Grid.SetRow(tb, r + 1);
                Grid.SetColumn(tb, 0);
                AvailabilityGrid.Children.Add(tb);
            }

            // Checkboxy z mapowaniem na availability[r,c]
            for (int r = 0; r < hours.Count; r++)
            {
                for (int c = 0; c < days.Count; c++)
                {
                    var cb = new CheckBox
                    {
                        IsChecked = availability[r, c],
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(3)
                    };

                    int rr = r, cc = c;
                    cb.Checked += (s, e) => availability[rr, cc] = true;
                    cb.Unchecked += (s, e) => availability[rr, cc] = false;

                    Grid.SetRow(cb, r + 1);
                    Grid.SetColumn(cb, c + 1);
                    AvailabilityGrid.Children.Add(cb);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Po prostu zamknij okno z DialogResult = true
            DialogResult = true;
            Close();
        }
    }
}
