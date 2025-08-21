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
        hour_time = hour.get("Hour", "")
        for day, slot in hour["Days"].items():
            if not slot or not slot.get("Subject"):
                continue
            
            # Kara za konflikty (mniejsze kary)
            penalty += self._check_conflicts(slot, day, hour_time, teacher_schedule, room_schedule, class_schedule)
            
            # Kara za zły typ sali (mniejsze kary)
            penalty += self._check_room_type(slot, individual)
            
            # Zliczanie godzin
            subject_hours[slot["Subject"]] += 1
            if slot.get("Class"):
                class_type = slot["Class"].split('_')[0]
                if class_type in daily_hours:
                    daily_hours[class_type][day] += 1
    
    # Kara za brakujące godziny (ważniejsze)
    penalty += self._check_missing_hours(individual.subjects, subject_hours) * 2
    
    # Kara za zbyt dużo godzin dziennie
    penalty += self._check_daily_hours(daily_hours)
    
    individual.fitness = penalty
    return penalty