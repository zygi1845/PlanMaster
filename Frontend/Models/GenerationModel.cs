using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Frontend.Models
{
    public class GenerationModel
    {
        public int TeacherCount { get; private set; }
        public int RoomCount { get; private set; }
        public int ClassCount { get; private set; }
        public int ClassTypeCount { get; private set; }

        public void LoadData()
        {
            // Wczytanie nauczycieli
            if (File.Exists("teachers.json"))
            {
                var teachersJson = File.ReadAllText("teachers.json");
                var teachers = JsonSerializer.Deserialize<JsonElement>(teachersJson);
                if (teachers.TryGetProperty("$values", out var values))
                {
                    TeacherCount = values.GetArrayLength();
                }
            }

            // Wczytanie sal
            if (File.Exists("rooms.json"))
            {
                var roomsJson = File.ReadAllText("rooms.json");
                var rooms = JsonSerializer.Deserialize<List<Room>>(roomsJson);
                RoomCount = rooms?.Count ?? 0;
            }

            // Wczytanie klas
            if (File.Exists("class.json"))
            {
                var classesJson = File.ReadAllText("class.json");
                var classTypes = JsonSerializer.Deserialize<List<ClassType>>(classesJson);
                ClassTypeCount = classTypes?.Count ?? 0;
                
                ClassCount = 0;
                if (classTypes != null)
                {
                    foreach (var classType in classTypes)
                    {
                        ClassCount += classType.Ilosc_klas_typu;
                    }
                }
            }
        }

        private class Room
        {
            public string RoomNumber { get; set; }
            public string RoomType { get; set; }
        }

        private class ClassType
        {
            public string NazwaTypu { get; set; }
            public int Ilosc_klas_typu { get; set; }
        }
    }
}