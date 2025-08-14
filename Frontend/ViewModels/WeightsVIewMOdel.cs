using Frontend.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Frontend.ViewModels
{
    public class WeightsViewModel : INotifyPropertyChanged
    {
        private readonly WeightsModel _model = new WeightsModel();

        public List<SubjectWeight> SubjectWeights => _model.SubjectWeights;
        public int MaxHoursPerDay
        {
            get => _model.MaxHoursPerDay;
            set { _model.MaxHoursPerDay = value; OnPropertyChanged(); }
        }
        public int MaxHoursWeight
        {
            get => _model.MaxHoursWeight;
            set { _model.MaxHoursWeight = value; OnPropertyChanged(); }
        }
        public int BreaksWeight
        {
            get => _model.BreaksWeight;
            set { _model.BreaksWeight = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }

        public WeightsViewModel()
        {
            _model.LoadData();
            SaveCommand = new RelayCommand(_ => 
            {
                _model.SaveData();
                MessageBox.Show("Zapisano zmiany!", "Sukces", MessageBoxButton.OK);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => _execute(parameter);
    }
}