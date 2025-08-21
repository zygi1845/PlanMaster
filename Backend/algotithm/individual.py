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
        self.fitness = float('inf')
        
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
        for hour_idx, hour in enumerate(self.timetable):
            for day in hour["Days"]:
                if random.random() > 0.3:  # 30% szans na zajęcia w slocie
                    continue
                    
                # Wybierz losowy przedmiot z niewykorzystanymi godzinami
                available_subjects = [s for s in self.subjects if s["Hours"] > 0]
                if not available_subjects:
                    continue
                subject = random.choice(available_subjects)
                
                # Znajdź dostępnych nauczycieli
                available_teachers = [
                    t for t in self.teachers 
                    if t["SelectedSubject"]["Name"] == subject["Name"]
                    and t["Availability"][hour_idx][["Monday","Tuesday","Wednesday","Thursday","Friday"].index(day)]
                ]
                if not available_teachers:
                    continue
                teacher = random.choice(available_teachers)
                
                # Znajdź odpowiednią salę
                suitable_rooms = [r for r in self.rooms if r["RoomType"] == subject["SubjectType"]]
                if not suitable_rooms:
                    continue
                room = random.choice(suitable_rooms)
                
                # Wybierz klasę
                class_type = random.choice(self.classes)
                class_name = f"{class_type['NazwaTypu']}_{random.randint(1, class_type['Ilosc_klas_typu'])}"
                
                # Przypisz zajęcia
                hour["Days"][day] = {
                    "Subject": subject["Name"],
                    "Teacher": teacher["Nazwisko"],
                    "Room": room["RoomNumber"],
                    "Class": class_name
                }
    
    def to_json(self, filename):
        """Zapisuje plan do JSON"""
        with open(filename, 'w') as f:
            json.dump({
                "classes": self.classes,
                "schedule": self.timetable,
                "rooms": self.rooms,
                "subjects": self.subjects,
                "teachers": self.teachers,
                "fitness": self.fitness
            }, f, indent=2)
    
    @classmethod
    def from_json(cls, filename):
        """Wczytuje plan z JSON"""
        with open(filename) as f:
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

        # Przetwarzanie danych
        for hour in self.timetable:
            for day, slot in hour["Days"].items():
                if not slot["Subject"]:
                    continue
                
                # Dane do zapisu
                wpis = {
                    "godzina": hour["Hour"],
                    "przedmiot": slot["Subject"],
                    "dzien": day
                }

                # Dla klas
                if slot["Class"]:
                    wpis_klasy = wpis.copy()
                    wpis_klasy["nauczyciel"] = slot["Teacher"]
                    wpis_klasy["sala"] = slot["Room"]
                    plany_klas[slot["Class"]][day].append(wpis_klasy)

                # Dla nauczycieli
                if slot["Teacher"]:
                    wpis_nauczyciela = wpis.copy()
                    wpis_nauczyciela["klasa"] = slot["Class"]
                    wpis_nauczyciela["sala"] = slot["Room"]
                    plany_nauczycieli[slot["Teacher"]][day].append(wpis_nauczyciela)

                # Dla sal
                if slot["Room"]:
                    wpis_sali = wpis.copy()
                    wpis_sali["klasa"] = slot["Class"]
                    wpis_sali["nauczyciel"] = slot["Teacher"]
                    plany_sal[slot["Room"]][day].append(wpis_sali)

        # Zapis do plików
        self._zapisz_slownik(plany_klas, base_path / "plany_klas.json")
        self._zapisz_slownik(plany_nauczycieli, base_path / "plany_nauczycieli.json")
        self._zapisz_slownik(plany_sal, base_path / "plany_sal.json")

    def _zapisz_slownik(self, dane, sciezka):
        """Pomocnicza funkcja do zapisu słowników"""
        with open(sciezka, 'w', encoding='utf-8') as f:
            json.dump(dane, f, indent=2, ensure_ascii=False)    