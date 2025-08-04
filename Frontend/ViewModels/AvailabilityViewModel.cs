using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontend.ViewModels
{
    public class AvailabilityViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private List<Hour> _availability = new();
        public List<Hour> Availability
        {
            get => _availability;
            set
            {
                _availability = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Availability)));
            }
        }

        public void LoadAvailability(string teacherId)
        {
            string availabilityPath = "availability.json";
            string hourPlanPath = "hourplan.json";

            List<Hour> hourList;

            if (File.Exists(availabilityPath))
            {
                string json = File.ReadAllText(availabilityPath);
                var allAvailability = JsonSerializer.Deserialize<List<TeacherAvailability>>(json) ?? new();
                var current = allAvailability.FirstOrDefault(t => t.TeacherId == teacherId);
                if (current != null)
                {
                    Availability = current.Availability;
                    return;
                }
            }

            if (File.Exists(hourPlanPath))
            {
                string json = File.ReadAllText(hourPlanPath);
                hourList = JsonSerializer.Deserialize<List<Hour>>(json) ?? new();
                foreach (var h in hourList)
                {
                    h.Available = false;
                }
                Availability = hourList;
            }
        }

        public void SaveAvailability(string teacherId)
        {
            string path = "availability.json";
            List<TeacherAvailability> allAvailability = new();

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                allAvailability = JsonSerializer.Deserialize<List<TeacherAvailability>>(json) ?? new();
            }

            var existing = allAvailability.FirstOrDefault(t => t.TeacherId == teacherId);
            if (existing != null)
            {
                existing.Availability = Availability;
            }
            else
            {
                allAvailability.Add(new TeacherAvailability
                {
                    TeacherId = teacherId,
                    Availability = Availability
                });
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string updatedJson = JsonSerializer.Serialize(allAvailability, options);
            File.WriteAllText(path, updatedJson);
        }
    }

    public class TeacherAvailability
    {
        [JsonPropertyName("teacherId")]
        public string TeacherId { get; set; } = string.Empty;

        [JsonPropertyName("availability")]
        public List<Hour> Availability { get; set; } = new();
    }

    public class Hour
    {
        [JsonPropertyName("day")]
        public string Day { get; set; } = string.Empty;

        [JsonPropertyName("hour")]
        public string HourTime { get; set; } = string.Empty;

        [JsonPropertyName("available")]
        public bool Available { get; set; }
    }
}
