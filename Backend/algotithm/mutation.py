import random
from copy import deepcopy

def mutacja(osobnik, szansa_mutacji=0.1):
    """Funkcja wykonująca mutację osobnika"""
    mutated = deepcopy(osobnik)
    dni = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]
    
    for godzina_idx, godzina in enumerate(mutated.timetable):
        for dzien in dni:
            slot = godzina["Days"][dzien]
            if random.random() < szansa_mutacji and slot and slot.get("Subject"):
                # Mutacja nauczyciela
                przedmiot = next((p for p in mutated.subjects if p["Name"] == slot.get("Subject", "")), None)
                if przedmiot:
                    dostepni_nauczyciele = []
                    for n in mutated.nauczyciele:
                        teacher_subject = n.get("SelectedSubject", {}).get("Name", "")
                        if teacher_subject == przedmiot["Name"]:
                            availability = n.get("Availability", [])
                            if (godzina_idx < len(availability) and 
                                dni.index(dzien) < len(availability[godzina_idx]) and
                                availability[godzina_idx][dni.index(dzien)]):
                                dostepni_nauczyciele.append(n)
                    
                    if dostepni_nauczyciele:
                        slot["Teacher"] = random.choice(dostepni_nauczyciele).get("Nazwisko", "")
                
                # Mutacja sali
                if przedmiot:
                    odpowiednie_sale = [
                        s for s in mutated.rooms 
                        if s.get("RoomType") == przedmiot.get("SubjectType")
                    ]
                    if odpowiednie_sale:
                        slot["Room"] = random.choice(odpowiednie_sale).get("RoomNumber", "")
                
                # Mutacja klasy
                if slot.get("Class") and "_" in slot["Class"]:
                    typ_klasy = slot["Class"].split("_")[0]
                    klasa_typ = next((k for k in mutated.classes if k.get("NazwaTypu") == typ_klasy), None)
                    if klasa_typ:
                        nr_klasy = random.randint(1, klasa_typ.get("Ilosc_klas_typu", 1))
                        slot["Class"] = f"{typ_klasy}_{nr_klasy}"
    
    return mutated