def calculate_fitness(individual):
    penalty = 0
    
    # Słowniki do śledzenia zajętości
    teacher_occupancy = {}
    room_occupancy = {}
    class_occupancy = {}
    
    for hour in individual.timetable:
        for day, slot in hour["Days"].items():
            if not slot["Subject"]:
                continue
                
            # Sprawdź konflikty nauczycieli
            teacher_key = f"{day}_{hour['Hour']}_{slot['Teacher']}"
            if teacher_key in teacher_occupancy:
                penalty += 100  # Duża kara za konflikt nauczyciela
            teacher_occupancy[teacher_key] = True
            
            # Sprawdź konflikty sal
            room_key = f"{day}_{hour['Hour']}_{slot['Room']}"
            if room_key in room_occupancy:
                penalty += 100  # Duża kara za konflikt sali
            room_occupancy[room_key] = True
            
            # Sprawdź konflikty klas
            class_key = f"{day}_{hour['Hour']}_{slot['Class']}"
            if class_key in class_occupancy:
                penalty += 100  # Duża kara za konflikt klasy
            class_occupancy[class_key] = True
            
            # Kara za nieodpowiedni typ sali
            subject = next((s for s in individual.subjects if s["Name"] == slot["Subject"]), None)
            room = next((r for r in individual.rooms if r["RoomNumber"] == slot["Room"]), None)
            if subject and room and subject["SubjectType"] != room["RoomType"]:
                penalty += 50
                
    # Kara za nieprzydzielone godziny przedmiotów
    subject_hours = {s["Name"]: s["Hours"] for s in individual.subjects}
    for s in subject_hours:
        assigned_hours = sum(1 for hour in individual.timetable 
                           for day in hour["Days"] 
                           if hour["Days"][day]["Subject"] == s)
        penalty += max(0, subject_hours[s] - assigned_hours) * 10
    
    individual.fitness = penalty
    return penalty