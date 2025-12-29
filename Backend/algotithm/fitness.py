"""Fitness calculation module with comprehensive penalties"""
from typing import Dict
from individual import Individual


class FitnessCalculator:
    """Calculate fitness with all penalties"""
    
    def __init__(self, constraints: Dict):
        self.constraints = constraints
        self.teachers_dict = constraints['teachers_dict']
        self.class_types = constraints['class_types']
        self.class_type_counts = constraints.get('class_type_counts', {})
        self.class_teachers = constraints.get('class_teachers', {})
        self.rooms = constraints['rooms']
        self.subjects = constraints['subjects']
        self.weights = constraints['weights']
        self.hours_per_day = constraints.get('hours_per_day', 8)
    
    def calculate(self, individual: Individual) -> float:
        """Calculate total fitness with all penalties"""
        penalty = 0
        
        # 1. KRYTYCZNE: Konflikty nauczycieli (-50 każdy)
        penalty += self._check_teacher_conflicts(individual)
        
        # 2. KRYTYCZNE: Nieprawidłowa suma godzin tygodniowo (-300 każdy)
        penalty += self._check_weekly_hour_totals(individual)
        
        # 3. BARDZO ZŁE: Luki w planach (używa BreaksWeight z config)
        penalty += self._check_gaps_in_schedule(individual)
        
        # 4. BARDZO ZŁE: Przekroczenie limitu godzin dziennie (używa MaxHoursWeight)
        penalty += self._check_daily_hour_limit(individual)
        
        # 5. ŹLE: Konflikty sal (-30 każdy)
        penalty += self._check_room_conflicts(individual)
        
        # 6. ŹLE: Nauczyciel niedostępny (-50)
        penalty += self._check_teacher_availability(individual)
        
        # 7. ŚREDNIO: Max godzin przedmiotu dziennie (wagi z SubjectWeights)
        penalty += self._check_max_subject_per_day(individual)
        
        # 8. LEKKO: Niezgodność typu sali (-30)
        penalty += self._check_room_type_match(individual)
        
        individual.fitness = -penalty
        return individual.fitness
    
    def _check_teacher_conflicts(self, individual: Individual) -> int:
        """Check teacher conflicts: -50 per conflict"""
        penalty = 0
        teacher_schedule = {}
        
        for lesson in individual.lessons:
            key = (lesson.teacher_id, lesson.day, lesson.hour)
            if key in teacher_schedule:
                penalty += 50
            teacher_schedule[key] = lesson
        
        return penalty
    
    def _check_room_conflicts(self, individual: Individual) -> int:
        """Check room conflicts: -30 per conflict"""
        penalty = 0
        room_schedule = {}
        
        for lesson in individual.lessons:
            key = (lesson.room_number, lesson.day, lesson.hour)
            if key not in room_schedule:
                room_schedule[key] = []
            room_schedule[key].append(lesson)
        
        for key, lessons_in_slot in room_schedule.items():
            unique_classes = set(l.class_type for l in lessons_in_slot)
            if len(unique_classes) > 1:
                conflicts = len(unique_classes) - 1
                penalty += 30 * conflicts
        
        return penalty
    
    def _check_gaps_in_schedule(self, individual: Individual) -> int:
        """Check for gaps: uses BreaksWeight from config (default 3, but multiply by penalty factor)"""
        penalty = 0
        breaks_weight = self.weights.get('BreaksWeight', 3)
        
        # Mnożnik aby było -40 zamiast -3
        penalty_multiplier = 13  # 3 * 13 ≈ 40
        
        for class_type in set(self.class_types):
            for day in range(6):
                hours = sorted([l.hour for l in individual.lessons 
                               if l.class_type == class_type and l.day == day])
                
                if len(hours) < 2:
                    continue
                
                first_lesson = hours[0]
                last_lesson = hours[-1]
                
                for hour in range(first_lesson + 1, last_lesson):
                    if hour not in hours:
                        penalty += breaks_weight * penalty_multiplier
        
        return penalty
    
    def _check_daily_hour_limit(self, individual: Individual) -> int:
        """Check daily hour limit: uses MaxHoursPerDay and MaxHoursWeight"""
        penalty = 0
        max_hours = self.weights.get('MaxHoursPerDay', 8)
        max_hours_weight = self.weights.get('MaxHoursWeight', 5)
        
        # Mnożnik aby było -35 zamiast -5
        penalty_multiplier = 7  # 5 * 7 = 35
        
        class_daily = {}
        for lesson in individual.lessons:
            key = (lesson.class_type, lesson.day)
            class_daily[key] = class_daily.get(key, 0) + 1
        
        for (class_type, day), count in class_daily.items():
            if count > max_hours:
                excess = count - max_hours
                penalty += excess * max_hours_weight * penalty_multiplier
        
        return penalty
    
    def _check_weekly_hour_totals(self, individual: Individual) -> int:
        """Check weekly hours per subject: -300 per mismatch"""
        penalty = 0
        
        for class_type in self.class_types:
            # Policz godziny każdego przedmiotu dla tej klasy
            subject_hours = {}
            for lesson in individual.lessons:
                if lesson.class_type == class_type:
                    subject = lesson.subject_name
                    subject_hours[subject] = subject_hours.get(subject, 0) + 1
            
            # Pobierz bazowy typ klasy (bez numeru)
            base_type = self.class_type_counts.get(class_type, class_type)
            
            # Sprawdź oczekiwane godziny z teachers->SelectedSubject
            teachers = self.class_teachers.get(class_type, [])
            
            for teacher in teachers:
                subject_data = teacher.get('SelectedSubject')
                if not subject_data:
                    continue
                
                # Sprawdź czy przedmiot jest dla tej klasy
                if base_type not in subject_data.get('ClassTypes', []):
                    continue
                
                subject_name = subject_data.get('Name')
                expected_hours = subject_data.get('Hours', 0)
                actual_hours = subject_hours.get(subject_name, 0)
                
                if actual_hours != expected_hours:
                    penalty += 300
        
        return penalty
    
    def _check_teacher_availability(self, individual: Individual) -> int:
        """Check teacher availability: -50 per violation"""
        penalty = 0
        
        for lesson in individual.lessons:
            teacher = self.teachers_dict.get(lesson.teacher_id)
            if teacher and lesson.day < len(teacher.get('Availability', [])):
                day_availability = teacher['Availability'][lesson.day]
                if lesson.hour < len(day_availability):
                    if not day_availability[lesson.hour]:
                        penalty += 50
        
        return penalty
    
    def _check_max_subject_per_day(self, individual: Individual) -> int:
        """Check max subject hours per day: uses SubjectWeights from config"""
        penalty = 0
        
        subject_daily = {}
        for lesson in individual.lessons:
            key = (lesson.class_type, lesson.subject_name, lesson.day)
            subject_daily[key] = subject_daily.get(key, 0) + 1
        
        # Używa SubjectWeights z Twojego config
        for sw in self.weights.get('SubjectWeights', []):
            subject_name = sw.get('Name')
            max_per_day = sw.get('MaxPerDay', 999)
            weight = sw.get('Weight', 5)
            
            for class_type in self.class_types:
                for day in range(6):
                    key = (class_type, subject_name, day)
                    count = subject_daily.get(key, 0)
                    if count > max_per_day:
                        excess = count - max_per_day
                        penalty += excess * weight
        
        return penalty
    
    def _check_room_type_match(self, individual: Individual) -> int:
        """Check room type matches subject type: -30 per mismatch"""
        penalty = 0
        
        for lesson in individual.lessons:
            room = next((r for r in self.rooms if r['RoomNumber'] == lesson.room_number), None)
            subject = next((s for s in self.subjects if s['Name'] == lesson.subject_name), None)
            
            if room and subject:
                if room.get('RoomType') != subject.get('SubjectType'):
                    penalty += 30
        
        return penalty