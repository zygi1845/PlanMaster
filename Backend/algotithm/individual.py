import json
import random
from copy import deepcopy
from collections import defaultdict
from pathlib import Path

class Individual:
    def __init__(self, classes, schedule, rooms, subjects, teachers):
        self.classes = classes
        self.schedule = schedule
        self.rooms = rooms
        self.subjects = subjects
        self.teachers = teachers
        self.timetable = self.generate_empty_timetable()
        self.fitness = float('inf')
        self.generate_random_schedule()
        
    def generate_empty_timetable(self):
        """Tworzy pusty plan lekcji"""
        return [{
            "Hour": slot["Hour"],
            "Days": {day: {
                "Subject": None,
                "Teacher": None,
                "Room": None,
                "Class": None
            } for day in ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]}
        } for slot in self.schedule]
    
    def generate_random_schedule(self):
        """Generuje losowy plan lekcji"""
        # Resetowanie godzin przedmiotów
        subject_hours = {subject["Name"]: subject["Hours"] for subject in self.subjects}
        total_hours_needed = sum(subject["Hours"] for subject in self.subjects)
        
        dni = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]
        available_slots = len(self.timetable) * len(dni)
        
        print(f"Potrzebne godziny: {total_hours_needed}, dostępne sloty: {available_slots}")
        
        # Próbuj umieścić zajęcia aż do wyczerpania godzin
        attempts = 0
        max_attempts = available_slots * 2
        
        while sum(subject_hours.values()) > 0 and attempts < max_attempts:
            attempts += 1
            
            # Wybierz losowy slot
            hour_idx = random.randint(0, len(self.timetable) - 1)
            day_name = random.choice(dni)
            slot = self.timetable[hour_idx]["Days"][day_name]
            
            # Jeśli slot jest już zajęty, spróbuj ponownie
            if slot["Subject"]:
                continue
                
            # Wybierz przedmiot z niewykorzystanymi godzinami
            available_subjects = [s for s in self.subjects if subject_hours[s["Name"]] > 0]
            if not available_subjects:
                break
                
            subject = random.choice(available_subjects)
            
            # Znajdź dostępnych nauczycieli dla tego przedmiotu
            available_teachers = []
            for teacher in self.teachers:
                teacher_subject_name = teacher.get("SelectedSubject", {}).get("Name", "")
                if teacher_subject_name == subject["Name"]:
                    availability = teacher.get("Availability", [])
                    if (isinstance(availability, list) and 
                        hour_idx < len(availability)):
                        hour_availability = availability[hour_idx]
                        if (isinstance(hour_availability, list) and 
                            dni.index(day_name) < len(hour_availability) and
                            hour_availability[dni.index(day_name)]):
                            available_teachers.append(teacher)
            
            if not available_teachers:
                continue
                
            teacher = random.choice(available_teachers)
            teacher_name = teacher.get("Nazwisko", "Unknown")
            
            # Znajdź odpowiednią salę
            suitable_rooms = [r for r in self.rooms if r.get("RoomType") == subject.get("SubjectType")]
            if not suitable_rooms:
                continue
                
            room = random.choice(suitable_rooms)
            room_number = room.get("RoomNumber", "")
            
            # Wybierz klasę odpowiedniego typu
            subject_class_types = subject.get("ClassTypes", [])
            available_class_types = [c for c in self.classes if c.get("NazwaTypu") in subject_class_types]
            if not available_class_types:
                continue
                
            class_type = random.choice(available_class_types)
            class_name = f"{class_type.get('NazwaTypu', '')}_{random.randint(1, class_type.get('Ilosc_klas_typu', 1))}"
            
            # Przypisz zajęcia
            slot["Subject"] = subject["Name"]
            slot["Teacher"] = teacher_name
            slot["Room"] = room_number
            slot["Class"] = class_name
            
            # Zmniejsz liczbę dostępnych godzin dla przedmiotu
            subject_hours[subject["Name"]] -= 1
        
        print(f"Wygenerowano zajęć: {total_hours_needed - sum(subject_hours.values())}/{total_hours_needed}")
    def to_json(self, filename):
        """Zapisuje plan do JSON"""
        with open(filename, 'w', encoding='utf-8') as f:
            json.dump({
                "classes": self.classes,
                "schedule": self.timetable,
                "rooms": self.rooms,
                "subjects": self.subjects,
                "teachers": self.teachers,
                "fitness": self.fitness
            }, f, indent=2, ensure_ascii=False)
    
    @classmethod
    def from_json(cls, filename):
        """Wczytuje plan z JSON"""
        with open(filename, encoding='utf-8') as f:
            data = json.load(f)
        ind = cls(data["classes"], data["schedule"], data["rooms"], data["subjects"], data["teachers"])
        ind.timetable = data["schedule"]
        ind.fitness = data.get("fitness", float('inf'))
        return ind
    
    def zapisz_plany(self, folder="plany"):
        """Zapisuje plany w trzech kategoriach: klasy, nauczyciele, sale"""
        base_path = Path(__file__).parent.parent.parent / folder
        base_path.mkdir(exist_ok=True)
        
        # Inicjalizacja struktur
        plany_klas = defaultdict(lambda: defaultdict(list))
        plany_nauczycieli = defaultdict(lambda: defaultdict(list))
        plany_sal = defaultdict(lambda: defaultdict(list))

        dni = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]
        
        # Przetwarzanie danych
        for hour in self.timetable:
            hour_time = hour.get("Hour", "")
            for day in dni:
                slot = hour["Days"][day]
                if not slot or not slot.get("Subject"):
                    continue
                
                # Dane do zapisu
                wpis = {
                    "godzina": hour_time,
                    "przedmiot": slot.get("Subject", ""),
                    "dzien": day
                }

                # Dla klas
                if slot.get("Class"):
                    wpis_klasy = wpis.copy()
                    wpis_klasy["nauczyciel"] = slot.get("Teacher", "")
                    wpis_klasy["sala"] = slot.get("Room", "")
                    plany_klas[slot["Class"]][day].append(wpis_klasy)

                # Dla nauczycieli
                if slot.get("Teacher"):
                    wpis_nauczyciela = wpis.copy()
                    wpis_nauczyciela["klasa"] = slot.get("Class", "")
                    wpis_nauczyciela["sala"] = slot.get("Room", "")
                    plany_nauczycieli[slot["Teacher"]][day].append(wpis_nauczyciela)

                # Dla sal
                if slot.get("Room"):
                    wpis_sali = wpis.copy()
                    wpis_sali["klasa"] = slot.get("Class", "")
                    wpis_sali["nauczyciel"] = slot.get("Teacher", "")
                    plany_sal[slot["Room"]][day].append(wpis_sali)

        # Zapis do plików
        self._zapisz_slownik(plany_klas, base_path / "plany_klas.json")
        self._zapisz_slownik(plany_nauczycieli, base_path / "plany_nauczycieli.json")
        self._zapisz_slownik(plany_sal, base_path / "plany_sal.json")

    def _zapisz_slownik(self, dane, sciezka):
        """Pomocnicza funkcja do zapisu słowników"""
        # Konwertuj defaultdict na zwykły dict
        converted_data = {}
        for key, value in dane.items():
            if hasattr(value, 'items'):
                converted_data[key] = dict(value)
            else:
                converted_data[key] = value
                
        with open(sciezka, 'w', encoding='utf-8') as f:
            json.dump(converted_data, f, indent=2, ensure_ascii=False)