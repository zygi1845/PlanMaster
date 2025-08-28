import json
import random
from copy import deepcopy

class Individual:
    def __init__(self, klasy, plan, sale, przedmioty, nauczyciele):
        self.klasy = klasy  # To są pojedyncze klasy (np. "infa_1", "matma_1", etc.)
        self.classes = klasy  # Dodajemy alias dla zgodności z kodem fitness
        self.plan = plan
        self.sale = sale
        self.przedmioty = przedmioty
        self.nauczyciele = nauczyciele
        self.timetable = self._generuj_plan()
        self.fitness = self._oblicz_fitness()

    def _generuj_plan(self):
        """Generuje losowy plan zajęć zgodny z hourplan.json."""
        timetable = deepcopy(self.plan)
        dni_tygodnia = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"]

        for hour in timetable:
            for day in dni_tygodnia:
                subject = random.choice(self.przedmioty) if self.przedmioty else None
                teacher = random.choice(self.nauczyciele) if self.nauczyciele else None
                classroom = random.choice(self.sale) if self.sale else None
                class_type = random.choice(self.klasy) if self.klasy else None

                # Bezpieczne pobranie nazwy klasy
                if class_type:
                    class_name = (
                        class_type.get("NazwaTypu") or
                        class_type.get("Typ") or
                        class_type.get("Name") or
                        "UNKNOWN"
                    )
                else:
                    class_name = None

                hour[day] = {
                    "Subject": subject.get("Name") if subject else None,
                    "Teacher": teacher.get("Nazwisko") if teacher else None,
                    "Room": classroom.get("RoomNumber") if classroom else None,
                    "Class": class_name
                }
        print("DEBUG: Generacja planu zakończona – wszystko git")
        return timetable

    def _oblicz_fitness(self):
        """Liczy fitness – uproszczona wersja."""
        score = 0
        dni_tygodnia = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"]

        for hour in self.timetable:
            for day in dni_tygodnia:
                slot = hour.get(day)
                if slot and slot.get("Subject"):
                    score += 1
        print(f"DEBUG: Fitness wyliczony: {score}")
        return score

    def zapisz_plany(self):
        """Zapisuje plany do plików JSON dla klas, nauczycieli i sal."""
        plany_klas = {}
        plany_nauczycieli = {}
        plany_sal = {}

        dni_tygodnia = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"]

        for hour in self.timetable:
            for day in dni_tygodnia:
                slot = hour.get(day)
                if slot:
                    klasa = slot.get("Class")
                    nauczyciel = slot.get("Teacher")
                    sala = slot.get("Room")

                    if klasa:
                        plany_klas.setdefault(klasa, []).append({day: slot})
                    if nauczyciel:
                        plany_nauczycieli.setdefault(nauczyciel, []).append({day: slot})
                    if sala:
                        plany_sal.setdefault(sala, []).append({day: slot})

        zajete = sum(1 for h in self.timetable for d in dni_tygodnia if h.get(d))
        print(f"DEBUG: timetable ma {len(self.timetable)} godzin, zajęte sloty: {zajete}")

        # Zapis do plików
        self._zapisz_slownik(plany_klas, "plany/plany_klas.json")
        self._zapisz_slownik(plany_nauczycieli, "plany/plany_nauczycieli.json")
        self._zapisz_slownik(plany_sal, "plany/plany_sal.json")
        print("DEBUG: Plany zapisane – wszystko git")

    @staticmethod
    def _zapisz_slownik(slownik, sciezka):
        import os
        os.makedirs(os.path.dirname(sciezka), exist_ok=True)
        with open(sciezka, "w", encoding="utf-8") as f:
            json.dump(slownik, f, indent=4, ensure_ascii=False)

# import json
# import random
# import os
# from copy import deepcopy
# from collections import defaultdict

# # Absolutna ścieżka do folderu głównego
# BASE_DIR = r"C:\Users\Zygmunt\Desktop\Semestr 6\PH_Projekt\Inzynierka"

# class Individual:
#     def __init__(self, classes, schedule, rooms, subjects, teachers):
#         self.classes = classes
#         self.schedule = schedule
#         self.rooms = rooms
#         self.subjects = subjects
#         self.teachers = teachers
#         self.timetable = self.generate_empty_timetable()
#         self.fitness = float('inf')
#         self.generate_random_schedule()
        
#     def generate_empty_timetable(self):
#         """Tworzy pusty plan lekcji"""
#         return [{
#             "Hour": slot["Hour"],
#             "Days": {day: {
#                 "Subject": None,
#                 "Teacher": None,
#                 "Room": None,
#                 "Class": None
#             } for day in ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]}
#         } for slot in self.schedule]
    
#     def generate_random_schedule(self):
#         """Generuje losowy plan lekcji z uwzględnieniem dostępności"""
#         # Resetowanie godzin przedmiotów
#         subject_hours = {subject["Name"]: subject["Hours"] for subject in self.subjects}
#         total_hours_needed = sum(subject["Hours"] for subject in self.subjects)
        
#         dni = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]
        
#         # Lista wszystkich możliwych slotów w losowej kolejności
#         all_slots = []
#         for hour_idx in range(len(self.timetable)):
#             for day in dni:
#                 all_slots.append((hour_idx, day))
#         random.shuffle(all_slots)
        
#         # Próbuj umieścić zajęcia w losowych slotach
#         for hour_idx, day in all_slots:
#             if sum(subject_hours.values()) <= 0:
#                 break
                
#             slot = self.timetable[hour_idx]["Days"][day]
            
#             # Jeśli slot jest już zajęty, pomiń
#             if slot["Subject"]:
#                 continue
                
#             # Wybierz przedmiot z niewykorzystanymi godzinami
#             available_subjects = [s for s in self.subjects if subject_hours[s["Name"]] > 0]
#             if not available_subjects:
#                 break
                
#             # Próbuj umieścić każdy przedmiot
#             random.shuffle(available_subjects)
#             for subject in available_subjects:
#                 # Znajdź dostępnych nauczycieli
#                 available_teachers = []
#                 for teacher in self.teachers:
#                     teacher_subject_name = teacher.get("SelectedSubject", {}).get("Name", "")
#                     if teacher_subject_name == subject["Name"]:
#                         availability = teacher.get("Availability", [])
#                         if (hour_idx < len(availability) and 
#                             dni.index(day) < len(availability[hour_idx]) and
#                             availability[hour_idx][dni.index(day)]):
#                             available_teachers.append(teacher)
                
#                 if not available_teachers:
#                     continue
                    
#                 # Znajdź odpowiednią salę
#                 suitable_rooms = [r for r in self.rooms if r.get("RoomType") == subject.get("SubjectType")]
#                 if not suitable_rooms:
#                     continue
                    
#                 # Wybierz klasę odpowiedniego typu
#                 subject_class_types = subject.get("ClassTypes", [])
#                 available_class_types = [c for c in self.classes if c.get("NazwaTypu") in subject_class_types]
#                 if not available_class_types:
#                     continue
                    
#                 # Wybierz losowo nauczyciela, salę i klasę
#                 teacher = random.choice(available_teachers)
#                 room = random.choice(suitable_rooms)
#                 class_type = random.choice(available_class_types)
#                 class_name = f"{class_type.get('NazwaTypu', '')}_{random.randint(1, class_type.get('Ilosc_klas_typu', 1))}"
                
#                 # Przypisz zajęcia
#                 slot["Subject"] = subject["Name"]
#                 slot["Teacher"] = teacher.get("Nazwisko", "")
#                 slot["Room"] = room.get("RoomNumber", "")
#                 slot["Class"] = class_name
                
#                 # Zmniejsz liczbę dostępnych godzin
#                 subject_hours[subject["Name"]] -= 1
#                 break
        
#         print(f"Wygenerowano zajęć: {total_hours_needed - sum(subject_hours.values())}/{total_hours_needed}")
    
#     def to_json(self, filename):
#         """Zapisuje plan do JSON"""
#         with open(filename, 'w', encoding='utf-8') as f:
#             json.dump({
#                 "classes": self.classes,
#                 "schedule": self.timetable,
#                 "rooms": self.rooms,
#                 "subjects": self.subjects,
#                 "teachers": self.teachers,
#                 "fitness": self.fitness
#             }, f, indent=2, ensure_ascii=False)
    
#     @classmethod
#     def from_json(cls, filename):
#         """Wczytuje plan z JSON"""
#         with open(filename, encoding='utf-8') as f:
#             data = json.load(f)
#         ind = cls(data["classes"], data["schedule"], data["rooms"], data["subjects"], data["teachers"])
#         ind.timetable = data["schedule"]
#         ind.fitness = data.get("fitness", float('inf'))
#         return ind
    
#     def zapisz_plany(self):
#         """Zapisuje plany w trzech kategoriach: klasy, nauczyciele, sale"""
#         folder_plany = os.path.join(BASE_DIR, 'plany')
#         os.makedirs(folder_plany, exist_ok=True)

#         plany_klas = defaultdict(lambda: defaultdict(list))
#         plany_nauczycieli = defaultdict(lambda: defaultdict(list))
#         plany_sal = defaultdict(lambda: defaultdict(list))

#         dni = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]

#         for hour in self.timetable:
#             hour_time = hour.get("Hour", "")
#             for day in dni:
#                 slot = hour["Days"][day]
#                 if not slot or not slot.get("Subject"):
#                     continue

#                 wpis = {
#                     "godzina": hour_time,
#                     "przedmiot": slot.get("Subject", ""),
#                     "dzien": day
#                 }

#                 if slot.get("Class"):
#                     wpis_klasy = wpis.copy()
#                     wpis_klasy["nauczyciel"] = slot.get("Teacher", "")
#                     wpis_klasy["sala"] = slot.get("Room", "")
#                     plany_klas[slot["Class"]][day].append(wpis_klasy)

#                 if slot.get("Teacher"):
#                     wpis_nauczyciela = wpis.copy()
#                     wpis_nauczyciela["klasa"] = slot.get("Class", "")
#                     wpis_nauczyciela["sala"] = slot.get("Room", "")
#                     plany_nauczycieli[slot["Teacher"]][day].append(wpis_nauczyciela)

#                 if slot.get("Room"):
#                     wpis_sali = wpis.copy()
#                     wpis_sali["klasa"] = slot.get("Class", "")
#                     wpis_sali["nauczyciel"] = slot.get("Teacher", "")
#                     plany_sal[slot["Room"]][day].append(wpis_sali)

#         # 🔹 Zapis osobnych plików dla każdej klasy
#         for klasa, plan in plany_klas.items():
#             sciezka = os.path.join(folder_plany, f"plan_{klasa}.json")
#             self._zapisz_slownik(plan, sciezka)

#         # 🔹 Jeden plik zbiorczy dla nauczycieli
#         self._zapisz_slownik(plany_nauczycieli, os.path.join(folder_plany, 'plany_nauczycieli.json'))

#         # 🔹 Jeden plik zbiorczy dla sal
#         self._zapisz_slownik(plany_sal, os.path.join(folder_plany, 'plany_sal.json'))


#     def _zapisz_slownik(self, dane, sciezka):
#         """Pomocnicza funkcja do zapisu słowników"""
#         # Konwertuj defaultdict na zwykły dict
#         converted_data = {}
#         for key, value in dane.items():
#             if hasattr(value, 'items'):
#                 converted_data[key] = dict(value)
#             else:
#                 converted_data[key] = value
                
#         with open(sciezka, 'w', encoding='utf-8') as f:
#             json.dump(converted_data, f, indent=2, ensure_ascii=False)