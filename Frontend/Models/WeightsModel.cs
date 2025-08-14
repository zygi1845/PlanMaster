using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Frontend.Models
{
    public class WeightsModel
    {
        private const string SubjectsFilePath = "subjects.json";
        private const string WeightsFilePath = "weights_config.json";

        public List<SubjectWeight> SubjectWeights { get; private set; } = new List<SubjectWeight>();
        public int MaxHoursPerDay { get; set; } = 8;
        public int MaxHoursWeight { get; set; } = 5;
        public int BreaksWeight { get; set; } = 5;

        public void LoadData()
        {
            try
            {
                if (File.Exists(SubjectsFilePath))
                {
                    var subjects = JsonSerializer.Deserialize<List<Subject>>(File.ReadAllText(SubjectsFilePath));
                    
                    if (File.Exists(WeightsFilePath))
                    {
                        var weightsData = JsonSerializer.Deserialize<WeightsData>(File.ReadAllText(WeightsFilePath));
                        MaxHoursPerDay = weightsData.MaxHoursPerDay;
                        MaxHoursWeight = weightsData.MaxHoursWeight;
                        BreaksWeight = weightsData.BreaksWeight;
                        
                        SubjectWeights = subjects.ConvertAll(s => 
                        {
                            var saved = weightsData.SubjectWeights.Find(sw => sw.Name == s.Name);
                            return new SubjectWeight
                            {
                                Name = s.Name,
                                Hours = s.Hours,
                                SubjectType = s.SubjectType,
                                MaxPerDay = saved?.MaxPerDay ?? 2,
                                Weight = saved?.Weight ?? 5
                            };
                        });
                    }
                    else
                    {
                        SubjectWeights = subjects.ConvertAll(s => new SubjectWeight
                        {
                            Name = s.Name,
                            Hours = s.Hours,
                            SubjectType = s.SubjectType,
                            MaxPerDay = 2,
                            Weight = 5
                        });
                    }
                }
            }
            catch { }
        }

        public void SaveData()
        {
            try
            {
                var data = new WeightsData
                {
                    SubjectWeights = SubjectWeights,
                    MaxHoursPerDay = MaxHoursPerDay,
                    MaxHoursWeight = MaxHoursWeight,
                    BreaksWeight = BreaksWeight
                };

                File.WriteAllText(WeightsFilePath, 
                    JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }

        private class WeightsData
        {
            public List<SubjectWeight> SubjectWeights { get; set; }
            public int MaxHoursPerDay { get; set; }
            public int MaxHoursWeight { get; set; }
            public int BreaksWeight { get; set; }
        }
    }

    public class Subject
    {
        public string Name { get; set; }
        public int Hours { get; set; }
        public string SubjectType { get; set; }
    }

    public class SubjectWeight : Subject
    {
        public int MaxPerDay { get; set; }
        public int Weight { get; set; }
    }
}