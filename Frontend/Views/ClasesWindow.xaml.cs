using System;
using System.Windows;
using System.Windows.Controls;
using INZYNIERKA.Frontend.ViewModels;
using INZYNIERKA.Frontend.Models;
using INZYNIERKA.Shared;


namespace Frontend.Views
{
    public partial class ClasesWindow : Window
    {
        private ClasesViewModel _viewModel = new ClasesViewModel();
        public ClasesWindow()
        {
            InitializeComponent();

            GlobalData.ClassCount = 17;
            DataContext = _viewModel;
        }

private void Zatwierdz_Click(object sender, RoutedEventArgs e)
{
    if (!int.TryParse(TypyKlasTextBox.Text, out int iloscTypow) || iloscTypow <= 0)
    {
        MessageBox.Show("Podaj poprawną ilość typów klas.");
        return;
    }

    if (!int.TryParse(IloscKlasTextBox.Text, out int iloscKlas) || iloscKlas <= 0)
    {
        MessageBox.Show("Podaj poprawną ilość klas.");
        return;
    }

    _viewModel.IloscTypow = iloscTypow;
    _viewModel.IloscKlas = iloscKlas;

    _viewModel.TypyKlas.Clear();
    for (int i = 0; i < iloscTypow; i++)
    {
        _viewModel.TypyKlas.Add(new ClassTypeModel { NazwaTypu = $"Nazwa typu: {i + 1}", IloscKlas = 0 });
    }

    TypyKlasItemsControl.ItemsSource = _viewModel.TypyKlas;

    // Pokaż drugi border z textboxami
    DynamicznePolaBorder.Visibility = Visibility.Visible;
}


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == TypyKlasTextBox && tb.Text == "Ilość Typów Klas")
                tb.Text = "";
            else if (tb == IloscKlasTextBox && tb.Text == "Ilość Klas")
                tb.Text = "";
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == TypyKlasTextBox && string.IsNullOrWhiteSpace(tb.Text))
                tb.Text = "Ilość Typów Klas";
            else if (tb == IloscKlasTextBox && string.IsNullOrWhiteSpace(tb.Text))
                tb.Text = "Ilość Klas";
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
