import json
import random
from copy import deepcopy

class Individual:
    def __init__(self, classes, schedule, rooms, subjects, teachers):
        self.classes = classes
        self.schedule = schedule
        self.rooms = rooms
        self.subjects = subjects
        self.teachers = teachers
        self.timetable = self.generate_empty_timetable()
        self.fitness = 0
        
    def generate_empty_timetable(self):
        """Tworzy pustą macierz planu lekcji"""
        timetable = []
        days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]
        
        for hour in self.schedule:
            hour_entry = {"Hour": hour["Hour"], "Days": {}}
            for day in days:
                hour_entry["Days"][day] = {
                    "Subject": None,
                    "Teacher": None,
                    "Room": None,
                    "Class": None
                }
            timetable.append(hour_entry)
        return timetable
    
    def generate_random_schedule(self):
        """Generuje losowy plan lekcji"""
        for hour_idx, hour in enumerate(self.timetable):
            for day in hour["Days"]:
                if random.random() > 0.7:  # 70% szans na zajęcia w danym slocie
                    continue
                    
                # Wybierz losowy przedmiot
                subject = random.choice([s for s in self.subjects if s["Hours"] > 0])
                
                # Znajdź nauczyciela dostępnego w tym czasie i nauczającego ten przedmiot
                available_teachers = [
                    t for t in self.teachers 
                    if t["Availability"][hour_idx][["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"].index(day)]
                    and t["Nazwisko"].lower() in subject["Name"].lower()  # Proste dopasowanie
                ]
                
                if not available_teachers:
                    continue
                    
                teacher = random.choice(available_teachers)
                
                # Wybierz losową salę odpowiedniego typu
                suitable_rooms = [r for r in self.rooms if r["RoomType"] == subject["SubjectType"]]
                if not suitable_rooms:
                    continue
                    
                room = random.choice(suitable_rooms)
                
                # Wybierz losową klasę
                class_ = random.choice(self.classes)
                
                # Przypisz zajęcia
                self.timetable[hour_idx]["Days"][day] = {
                    "Subject": subject["Name"],
                    "Teacher": teacher["Nazwisko"],
                    "Room": room["RoomNumber"],
                    "Class": class_["NazwaTypu"] + "_" + str(random.randint(1, class_["Ilosc_klas_typu"]))
                }
    
    def to_json(self, filename):
        """Zapisuje plan do pliku JSON"""
        with open(filename, 'w') as f:
            json.dump(self.timetable, f, indent=4)
    
    def from_json(self, filename):
        """Wczytuje plan z pliku JSON"""
        with open(filename, 'r') as f:
            self.timetable = json.load(f)