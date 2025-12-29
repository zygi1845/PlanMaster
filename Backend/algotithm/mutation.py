"""Mutation operators"""
import random
from typing import List, Dict
from individual import Individual


class MutationOperator:
    """Mutation operator"""
    
    def __init__(self, mutation_rate: float = 0.2, teachers_dict: Dict = None,
                 rooms: List[Dict] = None, subjects: List[Dict] = None, hours_per_day: int = 6):
        self.mutation_rate = mutation_rate
        self.teachers_dict = teachers_dict or {}
        self.rooms = rooms or []
        self.subjects = subjects or []
        self.hours_per_day = hours_per_day
    
    def mutate(self, individual: Individual):
        """Mutate individual"""
        if random.random() > self.mutation_rate or not individual.lessons:
            return
        
        lesson = random.choice(individual.lessons)
        
        if random.random() < 0.5:
            self._time_mutation(lesson)
        else:
            self._room_mutation(lesson)
    
    def _time_mutation(self, lesson):
        """Change time"""
        teacher = self.teachers_dict.get(lesson.teacher_id)
        if not teacher:
            return
        
        available = []
        for day in range(6):
            if day < len(teacher['Availability']):
                for hour in range(self.hours_per_day):
                    if hour < len(teacher['Availability'][day]):
                        if teacher['Availability'][day][hour]:
                            available.append((day, hour))
        
        if available:
            lesson.day, lesson.hour = random.choice(available)
    
    def _room_mutation(self, lesson):
        """Change room"""
        if not self.rooms:
            return
        
        subject = next((s for s in self.subjects if s['Name'] == lesson.subject_name), None)
        if subject:
            suitable = [r for r in self.rooms if r['RoomType'] == subject['SubjectType']]
            rooms = suitable if suitable else self.rooms
            if rooms:
                lesson.room_number = random.choice(rooms)['RoomNumber']

"""
from individual import Individual
import random
from copy import deepcopy

def mutat(osobnik: Individual, szansa_mutacji=0.1):
    mutated = deepcopy(osobnik)
    dni = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]

    for godzina_idx, godzina in enumerate(mutated.timetable):
        for dzien in dni:
            slot = godzina["Days"][dzien]
            if random.random() < szansa_mutacji and slot and slot.get("Subject"):
                # Mutacja nauczyciela
                przedmiot = next((p for p in mutated.przedmioty if p["Name"] == slot.get("Subject")), None)
                if przedmiot:
                    dostepni_nauczyciele = []
                    for n in mutated.nauczyciele:
                        teacher_subject = n.get("SelectedSubject", {}).get("Name", "")
                        if teacher_subject == przedmiot["Name"]:
                            availability = n.get("Availability", [])
                            day_idx = dni.index(dzien)
                            if godzina_idx < len(availability) and day_idx < len(availability[godzina_idx]):
                                if availability[godzina_idx][day_idx]:
                                    dostepni_nauczyciele.append(n)
                    if dostepni_nauczyciele:
                        nowy_nauczyciel = random.choice(dostepni_nauczyciele)
                        slot["Teacher"] = nowy_nauczyciel.get("Nazwisko")

                # Mutacja sali
                if przedmiot:
                    odpowiednie_sale = [r for r in mutated.sale if r.get("RoomType") == przedmiot.get("SubjectType")]
                    if odpowiednie_sale:
                        slot["Room"] = random.choice(odpowiednie_sale).get("RoomNumber")

                # Mutacja klasy
                if mutated.klasy:
                    nowa_klasa = random.choice(mutated.klasy)
                    slot["Class"] = nowa_klasa.get("NazwaTypu")

    return mutated
"""