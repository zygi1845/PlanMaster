using Frontend.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;


namespace Frontend.ViewModels
{
    public class HourPlanViewModel
    {
        public ObservableCollection<HourPlanRow> Rows { get; set; } = new ObservableCollection<HourPlanRow>();

        private readonly string savePath = "hourplan.json";

        public HourPlanViewModel()
        {
            LoadFromFile();
        }

        public void AddRow()
        {
            Rows.Add(new HourPlanRow());
        }

        public void RemoveRow(HourPlanRow row)
        {
            if (row != null) Rows.Remove(row);
        }

        public void SaveToFile()
        {
            var json = JsonSerializer.Serialize(Rows);
            File.WriteAllText(savePath, json);
        }

        public void LoadFromFile()
        {
            if (File.Exists(savePath))
            {
                var json = File.ReadAllText(savePath);
                var list = JsonSerializer.Deserialize<List<HourPlanRow>>(json);
                Rows.Clear();
                foreach (var row in list)
                    Rows.Add(row);
            }
            else
            {
                // domyślnie 10 godzin
                for (int i = 0; i < 10; i++)
                {
                    Rows.Add(new HourPlanRow
                    {
                        Hour = $"{i + 1}"
                    });
                }
            }
        }
    }
}
