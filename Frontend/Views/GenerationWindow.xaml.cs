using System.Windows;
using System.Windows.Input;
using System;
using System.Diagnostics;

namespace Frontend.Views
{
    public partial class GenerationWindow : Window
    {
        public static readonly RoutedUICommand GenerateAndNavigateCommand =
            new RoutedUICommand("Generate and navigate", "GenerateAndNavigate", typeof(GenerationWindow));

        public GenerationWindow()
        {
            try
            {
                Debug.WriteLine("GenerationWindow: Konstruktor START");
                InitializeComponent();
                Debug.WriteLine("GenerationWindow: InitializeComponent OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BŁĄD w konstruktorze: {ex.Message}");
                MessageBox.Show($"Błąd inicjalizacji okna:\n{ex.Message}\n\nStack:\n{ex.StackTrace}", 
                    "KRYTYCZNY BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void GenerateAndNavigateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("GenerateAndNavigateCommand_Executed: START");
            
            try
            {
                if (DataContext is ViewModels.GenerationViewModel vm)
                {
                    Debug.WriteLine("ViewModel found");
                    vm.GenerateTimetable((success, message) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show(message, success ? "Sukces" : "Błąd",
                                MessageBoxButton.OK, success ? MessageBoxImage.Information : MessageBoxImage.Error);
                            
                            if (success)
                            {
                                var plansWindow = new PlansWindow();
                                plansWindow.Show();
                                this.Close();
                            }
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BŁĄD w GenerateAndNavigateCommand: {ex.Message}");
                MessageBox.Show($"Błąd generowania:\n{ex.Message}\n\nStack:\n{ex.StackTrace}", 
                    "BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("GenerateCommand_Executed: START");
            
            try
            {
                if (DataContext is ViewModels.GenerationViewModel vm)
                {
                    vm.GenerateTimetable((success, message) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show(message, success ? "Sukces" : "Błąd",
                                MessageBoxButton.OK, success ? MessageBoxImage.Information : MessageBoxImage.Error);
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BŁĄD w GenerateCommand: {ex.Message}");
                MessageBox.Show($"Błąd:\n{ex.Message}", "BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (DataContext is ViewModels.GenerationViewModel vm)
                {
                    vm.Navigate(e.Parameter?.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BŁĄD w NavigateCommand: {ex.Message}");
                MessageBox.Show($"Błąd nawigacji:\n{ex.Message}", "BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========================================
        // NAWIGACJA Z DEBUGOWANIEM
        // ========================================
        
        private void HourPlanWindow_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("HourPlanWindow_Click: START");
            try
            {
                Debug.WriteLine("Tworzę HourPlanWindow...");
                var window = new HourPlanWindow();
                Debug.WriteLine("HourPlanWindow utworzone");
                
                window.Show();
                Debug.WriteLine("HourPlanWindow.Show() OK");
                
                this.Close();
                Debug.WriteLine("GenerationWindow.Close() OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BŁĄD w HourPlanWindow_Click: {ex.Message}");
                Debug.WriteLine($"Stack: {ex.StackTrace}");
                MessageBox.Show(
                    $"❌ BŁĄD NAWIGACJI do HourPlanWindow\n\n" +
                    $"Wiadomość: {ex.Message}\n\n" +
                    $"Typ: {ex.GetType().Name}\n\n" +
                    $"Stack:\n{ex.StackTrace}",
                    "BŁĄD KRYTYCZNY",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ClasesWindow_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ClasesWindow_Click: START");
            try
            {
                var window = new ClasesWindow();
                window.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BŁĄD w ClasesWindow_Click: {ex.Message}");
                MessageBox.Show($"❌ Błąd nawigacji:\n{ex.Message}", "BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SubjectsWindow_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SubjectsWindow_Click: START");
            try
            {
                var window = new SubjectsWindow();
                window.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BŁĄD w SubjectsWindow_Click: {ex.Message}");
                MessageBox.Show($"❌ Błąd nawigacji:\n{ex.Message}", "BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RoomsWindow_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("RoomsWindow_Click: START");
            try
            {
                var window = new RoomsWindow();
                window.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BŁĄD w RoomsWindow_Click: {ex.Message}");
                MessageBox.Show($"❌ Błąd nawigacji:\n{ex.Message}", "BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TeachersWindow_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("TeachersWindow_Click: START");
            try
            {
                var window = new TeachersWindow();
                window.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BŁĄD w TeachersWindow_Click: {ex.Message}");
                MessageBox.Show($"❌ Błąd nawigacji:\n{ex.Message}", "BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WeightsWindow_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("WeightsWindow_Click: START");
            try
            {
                var window = new WeightsWindow();
                window.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BŁĄD w WeightsWindow_Click: {ex.Message}");
                MessageBox.Show($"❌ Błąd nawigacji:\n{ex.Message}", "BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerationWindow_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("GenerationWindow_Click: Już na tym oknie");
            // Już jesteśmy tutaj - nic nie rób
        }
        
        private void PlansWindow_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("PlansWindow_Click: START");
            try
            {
                var window = new PlansWindow();
                window.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BŁĄD w PlansWindow_Click: {ex.Message}");
                MessageBox.Show($"❌ Błąd nawigacji:\n{ex.Message}", "BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}