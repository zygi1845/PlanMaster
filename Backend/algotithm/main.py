import json
import os
import sys
from pathlib import Path

BASE_DIR = r"C:\Users\Zygmunt\Desktop\Semestr 6\PH_Projekt\Inzynierka"

# --- Wczytywanie danych ---
def wczytaj_dane():
    sciezki = {
        'class': os.path.join(BASE_DIR, 'class.json'),
        'hourplan': os.path.join(BASE_DIR, 'hourplan.json'),
        'rooms': os.path.join(BASE_DIR, 'rooms.json'),
        'subjects': os.path.join(BASE_DIR, 'subjects.json'),
        'teachers': os.path.join(BASE_DIR, 'teachers.json')
    }

    # Klasy
    klasy_data = wczytaj_json(sciezki['class'], "class.json")
    plan = wczytaj_json(sciezki['hourplan'], "hourplan.json")
    sale = wczytaj_json(sciezki['rooms'], "rooms.json")
    przedmioty = wczytaj_json(sciezki['subjects'], "subjects.json")
    nauczyciele_data = wczytaj_json(sciezki['teachers'], "teachers.json")

    # Przetwarzanie nauczycieli
    nauczyciele = przetworz_nauczycieli(nauczyciele_data)

    # Tworzenie listy klas - POPRAWIONE
    klasy = []
    for typ in klasy_data:
        nazwa_typu = typ.get("NazwaTypu") or typ.get("Name") or "UNKNOWN"
        ilosc = typ.get("Ilosc_klas_typu", 1)
        for i in range(ilosc):
            klasy.append({"NazwaTypu": nazwa_typu, "Nazwa": f"{nazwa_typu}_{i+1}"})

    print(f"Liczba klas: {len(klasy)}, Liczba typów klas: {len(klasy_data)}")
    print(f"Liczba sal: {len(sale)}, Liczba przedmiotów: {len(przedmioty)}, Liczba nauczycieli: {len(nauczyciele)}")

    return klasy, klasy_data, plan, sale, przedmioty, nauczyciele


def wczytaj_json(path, nazwa):
    if not os.path.exists(path):
        raise FileNotFoundError(f"Brak pliku: {nazwa}")
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
        if isinstance(data, dict) and "$values" in data:
            data = data["$values"]
    print(f"Znaleziono plik: {nazwa}")
    return data


def przetworz_nauczycieli(dane):
    if isinstance(dane, dict) and "$values" in dane:
        dane = dane["$values"]

    nauczyciele = []
    for n in dane:
        availability = []
        if "Availability" in n:
            av = n["Availability"]
            if isinstance(av, dict) and "$values" in av:
                availability = av["$values"]
            elif isinstance(av, list):
                availability = av

        subject = {}
        if "SelectedSubject" in n:
            s = n["SelectedSubject"]
            if isinstance(s, dict):
                subject = {
                    "Name": s.get("Name", ""),
                    "Hours": s.get("Hours", 0),
                    "SubjectType": s.get("SubjectType", ""),
                    "ClassTypes": s.get("ClassTypes", {}).get("$values", []) if isinstance(s.get("ClassTypes"), dict) else s.get("ClassTypes", [])
                }

        nauczyciele.append({
            "Nr_pracownika": n.get("Nr_pracownika", 0),
            "Nazwisko": n.get("Nazwisko", ""),
            "Availability": availability,
            "SelectedSubject": subject
        })
    return nauczyciele


# --- Main ---
sys.path.append(os.path.dirname(__file__))
from genetic_algorithm import AlgorytmGenetyczny
from individual import Individual

def main():
    try:
        klasy, klasy_typy, plan, sale, przedmioty, nauczyciele = wczytaj_dane()

        print(f"Liczba nauczycieli: {len(nauczyciele)}")
        print(f"Liczba przedmiotów: {len(przedmioty)}")
        print(f"Liczba sal: {len(sale)}")
        print(f"Liczba typów klas: {len(klasy_typy)}")
        print(f"Liczba klas: {len(klasy)}")
        print(f"Liczba slotów czasowych: {len(plan)}")

        # Testowy osobnik
        test_individual = Individual(klasy, plan, sale, przedmioty, nauczyciele)
        print(f"DEBUG: Wygenerowany timetable, liczba slotów: {len(test_individual.timetable)}")
        print(f"Fitness testowego osobnika: {test_individual.fitness}")

        # Algorytm genetyczny
        alg = AlgorytmGenetyczny(
            klasy=klasy,
            plan=plan,
            sale=sale,
            przedmioty=przedmioty,
            nauczyciele=nauczyciele,
            rozmiar_populacji=10,
            generacje=5,
            szansa_krzyzowania=0.8,
            szansa_mutacji=0.3,
            elityzm=2
        )
        najlepszy = alg.ewoluuj()
        najlepszy.zapisz_plany()
        print("Pomyślnie zapisano plany: plany_klas.json, plany_nauczycieli.json, plany_sal.json")

    except Exception as e:
        print(f"Błąd: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    main()



# import json
# import sys
# import os

# # Absolutna ścieżka do folderu głównego
# BASE_DIR = r"C:\Users\Zygmunt\Desktop\Semestr 6\PH_Projekt\Inzynierka"

# def wczytaj_lub_utworz_plik(path, default_data):
#     """Wczytuje plik JSON, jeśli nie istnieje, tworzy go z default_data"""
#     if not os.path.exists(path):
#         with open(path, 'w', encoding='utf-8') as f:
#             json.dump(default_data, f, ensure_ascii=False, indent=4)
#         print(f"Nie znaleziono pliku {os.path.basename(path)}, utworzono nowy.")
#         return default_data
#     else:
#         with open(path, 'r', encoding='utf-8') as f:
#             print(f"Znaleziono plik: {os.path.basename(path)}")
#             return json.load(f)

# def wczytaj_dane():
#     """Wczytuje wszystkie dane z plików JSON w INZYNIERKA"""
#     sciezki = {
#         'classes': os.path.join(BASE_DIR, 'class.json'),
#         'hourplan': os.path.join(BASE_DIR, 'hourplan.json'),
#         'rooms': os.path.join(BASE_DIR, 'rooms.json'),
#         'subjects': os.path.join(BASE_DIR, 'subjects.json'),
#         'teachers': os.path.join(BASE_DIR, 'teachers.json')
#     }

#     klasy = wczytaj_lub_utworz_plik(sciezki['classes'], [])
#     plan = wczytaj_lub_utworz_plik(sciezki['hourplan'], [])
#     sale = wczytaj_lub_utworz_plik(sciezki['rooms'], [])
#     przedmioty = wczytaj_lub_utworz_plik(sciezki['subjects'], [])
#     nauczyciele_data = wczytaj_lub_utworz_plik(sciezki['teachers'], [])

#     nauczyciele = _przetworz_nauczycieli(nauczyciele_data)
    
#     return klasy, plan, sale, przedmioty, nauczyciele

# def _przetworz_nauczycieli(nauczyciele_data):
#     """Przetwarza strukturę nauczycieli z formatu Entity Framework"""
#     if isinstance(nauczyciele_data, dict) and '$values' in nauczyciele_data:
#         raw_teachers = nauczyciele_data['$values']
#     else:
#         raw_teachers = nauczyciele_data
    
#     nauczyciele = []
    
#     for teacher in raw_teachers:
#         availability = []
#         if 'Availability' in teacher:
#             availability_data = teacher['Availability']
#             if isinstance(availability_data, dict) and '$values' in availability_data:
#                 for hour_availability in availability_data['$values']:
#                     if isinstance(hour_availability, dict) and '$values' in hour_availability:
#                         availability.append(hour_availability['$values'])
#                     elif isinstance(hour_availability, list):
#                         availability.append(hour_availability)
#             elif isinstance(availability_data, list):
#                 availability = availability_data
        
#         selected_subject = {}
#         if 'SelectedSubject' in teacher:
#             subject_data = teacher['SelectedSubject']
#             if isinstance(subject_data, dict):
#                 if '$ref' in subject_data:
#                     ref_id = subject_data['$ref']
#                     for t in raw_teachers:
#                         if '$id' in t and t['$id'] == ref_id and 'SelectedSubject' in t:
#                             selected_subject = t['SelectedSubject']
#                             break
#                 else:
#                     selected_subject = subject_data
        
#         class_types = []
#         if 'ClassTypes' in selected_subject:
#             class_types_data = selected_subject['ClassTypes']
#             if isinstance(class_types_data, dict) and '$values' in class_types_data:
#                 class_types = class_types_data['$values']
#             elif isinstance(class_types_data, list):
#                 class_types = class_types_data
        
#         nauczyciel = {
#             'Nr_pracownika': teacher.get('Nr_pracownika', 0),
#             'Nazwisko': teacher.get('Nazwisko', ''),
#             'Availability': availability,
#             'SelectedSubject': {
#                 'Name': selected_subject.get('Name', ''),
#                 'Hours': selected_subject.get('Hours', 0),
#                 'SubjectType': selected_subject.get('SubjectType', ''),
#                 'ClassTypes': class_types
#             }
#         }
#         nauczyciele.append(nauczyciel)
    
#     return nauczyciele

# # Dodaj folder algotithm do ścieżki Pythona
# sys.path.append(os.path.dirname(__file__))

# from genetic_algorithm import AlgorytmGenetyczny
# from individual import Individual

# def main():
#     try:
#         klasy, plan, sale, przedmioty, nauczyciele = wczytaj_dane()
        
#         print(f"Liczba nauczycieli: {len(nauczyciele)}")
#         print(f"Liczba przedmiotów: {len(przedmioty)}")
#         print(f"Liczba sal: {len(sale)}")
        
#         liczba_typow_klas = len(klasy)
#         liczba_klas = sum(typ["Ilosc_klas_typu"] for typ in klasy)
#         print(f"Liczba typów klas: {liczba_typow_klas}")
#         print(f"Liczba klas: {liczba_klas}")
        
#         print(f"Liczba slotów czasowych: {len(plan)}")


#     except Exception as e:
#         print(f"Błąd: {str(e)}")
#         import traceback
#         traceback.print_exc()

# if __name__ == "__main__":
#     main()
