import random

def mutate(individual, mutation_rate=0.1):
    """Mutacja przez losową zmianę slotów"""
    for hour in individual.timetable:
        for day in hour["Days"]:
            if random.random() < mutation_rate:
                # Losowa zmiana nauczyciela
                if hour["Days"][day]["Teacher"]:
                    available_teachers = [
                        t for t in individual.teachers 
                        if t["Availability"][individual.timetable.index(hour)]
                        [["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"].index(day)]
                    ]
                    if available_teachers:
                        hour["Days"][day]["Teacher"] = random.choice(available_teachers)["Nazwisko"]
                
                # Losowa zmiana sali
                if hour["Days"][day]["Room"]:
                    suitable_rooms = [
                        r for r in individual.rooms 
                        if r["RoomType"] == next(
                            (s["SubjectType"] for s in individual.subjects 
                             if s["Name"] == hour["Days"][day]["Subject"]), "")
                    ]
                    if suitable_rooms:
                        hour["Days"][day]["Room"] = random.choice(suitable_rooms)["RoomNumber"]
    
    return individual