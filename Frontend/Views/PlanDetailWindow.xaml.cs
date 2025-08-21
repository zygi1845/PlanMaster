using Frontend.Models;
using System.Windows;
using System.Collections.Generic;

namespace Frontend.Views
{
    public partial class PlanDetailWindow : Window
    {
        public new string Title { get; set; } // Dodaj 'new' bo ukrywasz właściwość z klasy bazowej
        public List<PlansModel> ScheduleEntries { get; set; }

        public PlanDetailWindow(string itemName, string title, Dictionary<string, List<PlansModel>> scheduleData)
        {
            InitializeComponent();
            
            this.Title = title;
            ScheduleEntries = new List<PlansModel>();

            // Konwersja danych do płaskiej listy
            foreach (var day in scheduleData)
            {
                foreach (var entry in day.Value)
                {
                    ScheduleEntries.Add(entry);
                }
            }

            DataContext = this;
        }
    }
}