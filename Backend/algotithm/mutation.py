import random
from copy import deepcopy

def mutacja(osobnik, szansa_mutacji=0.1):
    """Funkcja wykonująca mutację osobnika"""
    for godzina_idx, godzina in enumerate(osobnik.timetable):
        for dzien, slot in godzina["Days"].items():
            if random.random() < szansa_mutacji and slot["Subject"]:
                # Mutacja nauczyciela
                przedmiot = next((p for p in osobnik.przedmioty if p["Name"] == slot["Subject"]), None)
                if przedmiot:
                    dostepni_nauczyciele = [
                        n for n in osobnik.nauczyciele 
                        if n["SelectedSubject"]["Name"] == przedmiot["Name"]
                        and n["Availability"][godzina_idx][list(godzina["Days"].keys()).index(dzien)]
                    ]
                    if dostepni_nauczyciele:
                        slot["Teacher"] = random.choice(dostepni_nauczyciele)["Nazwisko"]
                
                # Mutacja sali
                odpowiednie_sale = [
                    s for s in osobnik.sale 
                    if s["RoomType"] == przedmiot["SubjectType"]
                ]
                if odpowiednie_sale:
                    slot["Room"] = random.choice(odpowiednie_sale)["RoomNumber"]
                
                # Mutacja klasy
                if "_" in slot["Class"]:
                    typ_klasy = slot["Class"].split("_")[0]
                    klasa_typ = next((k for k in osobnik.klasy if k["NazwaTypu"] == typ_klasy), None)
                    if klasa_typ:
                        nr_klasy = random.randint(1, klasa_typ["Ilosc_klas_typu"])
                        slot["Class"] = f"{typ_klasy}_{nr_klasy}"
    
    return osobnik