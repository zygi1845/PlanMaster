using System.Collections.Generic;
using System.Windows;

namespace Frontend.Views
{
    public partial class HourPlanWindow : Window
    {
        private List<TableRow> data = new List<TableRow>();

        public HourPlanWindow()
        {
            InitializeComponent();
            InitializeTable();
        }

        private void InitializeTable()
        {
            for (int i = 0; i < 10; i++)
            {
                data.Add(new TableRow
                {
                    Col1 = $"R{i}C1",
                    Col2 = $"R{i}C2",
                    Col3 = $"R{i}C3",
                    Col4 = $"R{i}C4",
                    Col5 = $"R{i}C5",
                    Col6 = $"R{i}C6",
                    Col7 = $"R{i}C7",
                    Col8 = $"R{i}C8"
                });
            }

            MyDataGrid.ItemsSource = data;
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            data.Add(new TableRow());
            MyDataGrid.Items.Refresh();
        }

        private void DeleteRow_Click(object sender, RoutedEventArgs e)
        {
            if (MyDataGrid.SelectedItem is TableRow selectedRow)
            {
                data.Remove(selectedRow);
                MyDataGrid.Items.Refresh();
            }
        }

        public class TableRow
        {
            public string Col1 { get; set; }
            public string Col2 { get; set; }
            public string Col3 { get; set; }
            public string Col4 { get; set; }
            public string Col5 { get; set; }
            public string Col6 { get; set; }
            public string Col7 { get; set; }
            public string Col8 { get; set; }
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
