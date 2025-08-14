import json
from pathlib import Path

class FitnessCalculator:
    def __init__(self):
        self.weights = self._load_weights()
    
    def _load_weights(self):
        with open(Path(__file__).parent.parent.parent / 'weights_config.json') as f:
            return json.load(f)
    
    def calculate(self, individual):
        penalty = 0
        
        # Śledzenie zajętości
        teacher_schedule = {}
        room_schedule = {}
        class_schedule = {}
        subject_hours = {s["Name"]: 0 for s in individual.subjects}
        daily_hours = {c["NazwaTypu"]: {day: 0 for day in ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]} 
                      for c in individual.classes}

        for hour_idx, hour in enumerate(individual.timetable):
            for day, slot in hour["Days"].items():
                if not slot["Subject"]:
                    continue
                
                # Kara za konflikty
                penalty += self._check_conflicts(slot, day, hour['Hour'], teacher_schedule, room_schedule, class_schedule)
                
                # Kara za zły typ sali
                penalty += self._check_room_type(slot, individual)
                
                # Zliczanie godzin
                subject_hours[slot["Subject"]] += 1
                class_type = slot["Class"].split('_')[0]
                daily_hours[class_type][day] += 1
        
        # Kara za brakujące godziny
        penalty += self._check_missing_hours(individual.subjects, subject_hours)
        
        # Kara za zbyt dużo godzin dziennie
        penalty += self._check_daily_hours(daily_hours)
        
        individual.fitness = penalty
        return penalty
    
    def _check_conflicts(self, slot, day, hour, teachers, rooms, classes):
        penalty = 0
        teacher_key = f"{slot['Teacher']}_{day}_{hour}"
        room_key = f"{slot['Room']}_{day}_{hour}"
        class_key = f"{slot['Class']}_{day}_{hour}"
        
        if teacher_key in teachers:
            penalty += self.weights['MaxHoursWeight'] * 2  # Większa kara za konflikt nauczyciela
        teachers[teacher_key] = True
        
        if room_key in rooms:
            penalty += self.weights['MaxHoursWeight']
        rooms[room_key] = True
        
        if class_key in classes:
            penalty += self.weights['MaxHoursWeight']
        classes[class_key] = True
        
        return penalty
    
    def _check_room_type(self, slot, individual):
        subject = next((s for s in individual.subjects if s["Name"] == slot["Subject"]), None)
        room = next((r for r in individual.rooms if r["RoomNumber"] == slot["Room"]), None)
        
        if subject and room and subject["SubjectType"] != room["RoomType"]:
            subject_weight = next((w["Weight"] for w in self.weights["SubjectWeights"] 
                                 if w["Name"] == subject["Name"]), 5)
            return subject_weight * 2
        return 0
    
    def _check_missing_hours(self, subjects, assigned_hours):
        penalty = 0
        for subject in subjects:
            required = subject["Hours"]
            assigned = assigned_hours.get(subject["Name"], 0)
            subject_weight = next((w["Weight"] for w in self.weights["SubjectWeights"] 
                                 if w["Name"] == subject["Name"]), 5)
            penalty += max(0, required - assigned) * subject_weight
        return penalty
    
    def _check_daily_hours(self, daily_hours):
        penalty = 0
        for class_type, days in daily_hours.items():
            for day, hours in days.items():
                if hours > self.weights['MaxHoursPerDay']:
                    penalty += (hours - self.weights['MaxHoursPerDay']) * self.weights['MaxHoursWeight']
        return penalty