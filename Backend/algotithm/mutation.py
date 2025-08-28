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
