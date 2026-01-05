using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System;
using System.Diagnostics;

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
            try
            {
                Debug.WriteLine("GenerationModel.LoadData: START");
                
                // Wczytanie nauczycieli
                TeacherCount = LoadTeacherCount();
                Debug.WriteLine($"Nauczyciele: {TeacherCount}");

                // Wczytanie sal
                RoomCount = LoadRoomCount();
                Debug.WriteLine($"Sale: {RoomCount}");

                // Wczytanie klas
                LoadClassData();
                Debug.WriteLine($"Klasy: {ClassCount}, Typy: {ClassTypeCount}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BŁĄD w LoadData: {ex.Message}");
                throw; // Przekaż błąd do ViewModelu
            }
        }

        private int LoadTeacherCount()
        {
            try
            {
                if (!File.Exists("teachers.json"))
                {
                    Debug.WriteLine("teachers.json NIE ISTNIEJE");
                    return 0;
                }

                var json = File.ReadAllText("teachers.json");
                Debug.WriteLine($"teachers.json wczytany, rozmiar: {json.Length}");
                
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    JsonElement teachersArray;

                    // Sprawdź czy to format C# ($values) czy czysty JSON
                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("$values", out var values))
                    {
                        Debug.WriteLine("Format C# z $values");
                        teachersArray = values;
                    }
                    else if (root.ValueKind == JsonValueKind.Array)
                    {
                        Debug.WriteLine("Format czysty array");
                        teachersArray = root;
                    }
                    else
                    {
                        Debug.WriteLine($"Nieznany format JSON, ValueKind: {root.ValueKind}");
                        return 0;
                    }

                    // ✅ POPRAWKA: Zbierz unikalne Nr_pracownika zamiast liczyć wszystkie wpisy
                    var uniqueTeacherIds = new HashSet<int>();
                    
                    foreach (var teacher in teachersArray.EnumerateArray())
                    {
                        if (teacher.TryGetProperty("Nr_pracownika", out var idElement))
                        {
                            uniqueTeacherIds.Add(idElement.GetInt32());
                        }
                    }

                    int uniqueCount = uniqueTeacherIds.Count;
                    Debug.WriteLine($"Znaleziono {teachersArray.GetArrayLength()} wpisów, {uniqueCount} unikalnych nauczycieli");
                    
                    return uniqueCount;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BŁĄD w LoadTeacherCount: {ex.Message}");
                return 0; // Zwróć 0 zamiast crashować
            }
        }

        private int LoadRoomCount()
        {
            try
            {
                if (!File.Exists("rooms.json"))
                {
                    Debug.WriteLine("rooms.json NIE ISTNIEJE");
                    return 0;
                }

                var json = File.ReadAllText("rooms.json");
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("$values", out var values))
                    {
                        return values.GetArrayLength();
                    }
                    else if (root.ValueKind == JsonValueKind.Array)
                    {
                        return root.GetArrayLength();
                    }
                    
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BŁĄD w LoadRoomCount: {ex.Message}");
                return 0;
            }
        }

        private void LoadClassData()
        {
            try
            {
                if (!File.Exists("class.json"))
                {
                    Debug.WriteLine("class.json NIE ISTNIEJE");
                    ClassCount = 0;
                    ClassTypeCount = 0;
                    return;
                }

                var json = File.ReadAllText("class.json");
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    JsonElement classArray;

                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("$values", out var values))
                    {
                        classArray = values;
                    }
                    else if (root.ValueKind == JsonValueKind.Array)
                    {
                        classArray = root;
                    }
                    else
                    {
                        ClassCount = 0;
                        ClassTypeCount = 0;
                        return;
                    }

                    ClassTypeCount = classArray.GetArrayLength();
                    ClassCount = 0;

                    foreach (var classType in classArray.EnumerateArray())
                    {
                        if (classType.TryGetProperty("Ilosc_klas_typu", out var countElement))
                        {
                            ClassCount += countElement.GetInt32();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BŁĄD w LoadClassData: {ex.Message}");
                ClassCount = 0;
                ClassTypeCount = 0;
            }
        }
    }
}