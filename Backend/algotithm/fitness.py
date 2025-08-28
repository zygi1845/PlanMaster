from copy import deepcopy
import random

class FitnessCalculator:
    dni = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]

    def __init__(self, subjects, teachers, rooms, classes):
        self.subjects = subjects
        self.teachers = teachers
        self.rooms = rooms
        self.classes = classes  # Typy klas np. "infa", "matma"

        self.weights = {
            'teacher_conflict': 10,
            'room_conflict': 8,
            'class_conflict': 10,
            'wrong_room_type': 5,
            'missing_hours': 15,
            'excess_daily_hours': 7,
            'teacher_availability': 12,
            'consecutive_hours': 3
        }

    def calculate_individual(self, individual):
        penalty = 0

        teacher_schedule = {}
        room_schedule = {}
        class_schedule = {}
        subject_hours = {s["Name"]: s["Hours"] for s in self.subjects}

        # Liczymy godziny dziennie dla typów klas
        daily_hours = {}
        for c in self.classes:
            class_type = c.get("NazwaTypu") or c.get("Typ") or c.get("Name") or "UNKNOWN"
            daily_hours[class_type] = {day: 0 for day in self.dni}

        for hour_idx, hour in enumerate(individual.timetable):
            for day, slot in hour.items():
                if not slot or not isinstance(slot, dict) or not slot.get("Subject"):
                    continue

                penalty += self._check_conflicts(slot, day, hour_idx, teacher_schedule, room_schedule, class_schedule)
                penalty += self._check_room_type(slot, individual)
                penalty += self._check_teacher_availability(slot, hour_idx, day, individual)

                if slot["Subject"] in subject_hours:
                    subject_hours[slot["Subject"]] -= 1

                if slot.get("Class"):
                    class_type = slot["Class"].split('_')[0]
                    if class_type in daily_hours:
                        daily_hours[class_type][day] += 1

        penalty += self._check_missing_hours(subject_hours)
        penalty += self._check_daily_hours(daily_hours)
        penalty += self._check_consecutive_hours(individual)

        individual.fitness = penalty
        return penalty

    def calculate_population(self, population):
        for ind in population:
            self.calculate_individual(ind)

    def _check_conflicts(self, slot, day, hour_idx, teacher_schedule, room_schedule, class_schedule):
        penalty = 0
        teacher = slot.get("Teacher")
        room = slot.get("Room")
        class_name = slot.get("Class")

        if teacher:
            key = f"{teacher}_{day}_{hour_idx}"
            if key in teacher_schedule:
                penalty += self.weights['teacher_conflict']
            else:
                teacher_schedule[key] = True

        if room:
            key = f"{room}_{day}_{hour_idx}"
            if key in room_schedule:
                penalty += self.weights['room_conflict']
            else:
                room_schedule[key] = True

        if class_name:
            key = f"{class_name}_{day}_{hour_idx}"
            if key in class_schedule:
                penalty += self.weights['class_conflict']
            else:
                class_schedule[key] = True

        return penalty

    def _check_room_type(self, slot, individual):
        if not slot.get("Subject") or not slot.get("Room"):
            return 0

        subject = next((s for s in individual.przedmioty if s["Name"] == slot["Subject"]), None)
        room = next((r for r in individual.sale if r["RoomNumber"] == slot["Room"]), None)

        if subject and room and subject.get("SubjectType") != room.get("RoomType"):
            return self.weights['wrong_room_type']
        return 0

    def _check_teacher_availability(self, slot, hour_idx, day, individual):
        if not slot.get("Teacher"):
            return 0

        teacher = next((t for t in individual.nauczyciele if t.get("Nazwisko") == slot["Teacher"]), None)
        if not teacher or not teacher.get("Availability"):
            return self.weights['teacher_availability']

        availability = teacher["Availability"]
        day_idx = self.dni.index(day)

        # BEZPIECZNE sprawdzenie dostępności
        try:
            if isinstance(availability, dict):
                if not availability.get(day, [])[hour_idx]:
                    return self.weights['teacher_availability']
            elif isinstance(availability, list):
                if hour_idx >= len(availability):
                    return self.weights['teacher_availability']
                if day_idx >= len(availability[hour_idx]):
                    return self.weights['teacher_availability']
                if not availability[hour_idx][day_idx]:
                    return self.weights['teacher_availability']
            else:
                return self.weights['teacher_availability']
        except Exception:
            return self.weights['teacher_availability']

        return 0


    def _check_missing_hours(self, subject_hours):
        penalty = 0
        for subject, hours_left in subject_hours.items():
            if hours_left > 0:
                penalty += hours_left * self.weights['missing_hours']
            elif hours_left < 0:
                penalty += abs(hours_left) * self.weights['missing_hours'] * 0.5
        return penalty

    def _check_daily_hours(self, daily_hours):
        penalty = 0
        for class_type, days in daily_hours.items():
            for day, hours in days.items():
                if hours > 6:
                    penalty += (hours - 6) * self.weights['excess_daily_hours']
        return penalty

    def _check_consecutive_hours(self, individual):
        penalty = 0
        for c in self.classes:
            class_type = c.get("NazwaTypu") or c.get("Typ") or c.get("Name") or "UNKNOWN"
            for day in self.dni:
                consecutive_count = 0
                for hour in individual.timetable:
                    slot = hour.get(day)
                    if slot and slot.get("Class") and slot["Class"].startswith(class_type):
                        consecutive_count += 1
                    else:
                        if consecutive_count > 4:
                            penalty += (consecutive_count - 4) * self.weights['consecutive_hours']
                        consecutive_count = 0
                if consecutive_count > 4:
                    penalty += (consecutive_count - 4) * self.weights['consecutive_hours']
        return penalty
