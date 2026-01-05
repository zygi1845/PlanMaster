using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Frontend.Views
{
    public partial class AvailabilityWindow : Window
    {
        public bool[,] AvailabilityData { get; private set; }
        public List<string> Hours { get; }
        public List<string> Days { get; }

        public AvailabilityWindow(bool[,] existingAvailability, List<string> hours, List<string> days)
        {
            InitializeComponent();

            Hours = hours;
            Days = days;
            AvailabilityData = existingAvailability ?? new bool[hours.Count, days.Count];

            GenerateGrid();
        }

        private void GenerateGrid()
        {
            // Dodaj wiersze dla godzin
            for (int i = 0; i < Hours.Count; i++)
            {
                AvailabilityGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
            }

            // Wypełnij siatkę
            for (int row = 0; row < Hours.Count; row++)
            {
                // Godziny w pierwszej kolumnie
                var hourText = new TextBlock
                {
                    Text = Hours[row],
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(hourText, 0);
                Grid.SetRow(hourText, row);
                AvailabilityGrid.Children.Add(hourText);

                // Komórki dostępności (5 dni)
                for (int col = 0; col < 5; col++)
                {
                    var cell = new Border
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Background = AvailabilityData[row, col] ? Brushes.DodgerBlue : Brushes.White,
                        Tag = new System.Windows.Point(row, col)
                    };

                    cell.MouseLeftButtonDown += (s, e) =>
                    {
                        var border = (Border)s;
                        var pos = (System.Windows.Point)border.Tag;
                        AvailabilityData[(int)pos.X, (int)pos.Y] = !AvailabilityData[(int)pos.X, (int)pos.Y];
                        border.Background = AvailabilityData[(int)pos.X, (int)pos.Y] ? Brushes.DodgerBlue : Brushes.White;
                    };

                    Grid.SetColumn(cell, col + 1);
                    Grid.SetRow(cell, row);
                    AvailabilityGrid.Children.Add(cell);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}