import json
import sys
from pathlib import Path

def wczytaj_dane():
    """Wczytuje dane z plików JSON"""
    base_path = Path(__file__).parent.parent.parent
    
    sciezki = {
        'classes': base_path / 'classes.json',
        'hourplan': base_path / 'hourplan.json',
        'rooms': base_path / 'rooms.json',
        'subjects': base_path / 'subjects.json',
        'teachers': base_path / 'teachers.json'
    }
    
    for nazwa, sciezka in sciezki.items():
        if not sciezka.exists():
            raise FileNotFoundError(f"Brak pliku: {sciezka}")
    
    with open(sciezki['classes'], encoding='utf-8') as f:
        klasy = json.load(f)
    with open(sciezki['hourplan'], encoding='utf-8') as f:
        plan = json.load(f)
    with open(sciezki['rooms'], encoding='utf-8') as f:
        sale = json.load(f)
    with open(sciezki['subjects'], encoding='utf-8') as f:
        przedmioty = json.load(f)
    with open(sciezki['teachers'], encoding='utf-8') as f:
        nauczyciele_data = json.load(f)
    
    # Przetworzenie danych nauczycieli z formatu $values
    nauczyciele = _przetworz_nauczycieli(nauczyciele_data)
        
    return klasy, plan, sale, przedmioty, nauczyciele

def _przetworz_nauczycieli(nauczyciele_data):
    """Przetwarza strukturę nauczycieli z formatu Entity Framework"""
    if isinstance(nauczyciele_data, dict) and '$values' in nauczyciele_data:
        raw_teachers = nauczyciele_data['$values']
    else:
        raw_teachers = nauczyciele_data
    
    nauczyciele = []
    
    for teacher in raw_teachers:
        # Przetwórz dostępność
        availability = []
        if 'Availability' in teacher:
            availability_data = teacher['Availability']
            if isinstance(availability_data, dict) and '$values' in availability_data:
                for hour_availability in availability_data['$values']:
                    if isinstance(hour_availability, dict) and '$values' in hour_availability:
                        availability.append(hour_availability['$values'])
                    elif isinstance(hour_availability, list):
                        availability.append(hour_availability)
            elif isinstance(availability_data, list):
                availability = availability_data
        
        # Przetwórz wybrany przedmiot
        selected_subject = {}
        if 'SelectedSubject' in teacher:
            subject_data = teacher['SelectedSubject']
            if isinstance(subject_data, dict):
                if '$ref' in subject_data:
                    # To jest referencja, znajdź rzeczywisty obiekt
                    ref_id = subject_data['$ref']
                    for t in raw_teachers:
                        if '$id' in t and t['$id'] == ref_id and 'SelectedSubject' in t:
                            selected_subject = t['SelectedSubject']
                            break
                else:
                    selected_subject = subject_data
        
        # Przetwórz typy klas
        class_types = []
        if 'ClassTypes' in selected_subject:
            class_types_data = selected_subject['ClassTypes']
            if isinstance(class_types_data, dict) and '$values' in class_types_data:
                class_types = class_types_data['$values']
            elif isinstance(class_types_data, list):
                class_types = class_types_data
        
        nauczyciel = {
            'Nr_pracownika': teacher.get('Nr_pracownika', 0),
            'Nazwisko': teacher.get('Nazwisko', ''),
            'Availability': availability,
            'SelectedSubject': {
                'Name': selected_subject.get('Name', ''),
                'Hours': selected_subject.get('Hours', 0),
                'SubjectType': selected_subject.get('SubjectType', ''),
                'ClassTypes': class_types
            }
        }
        nauczyciele.append(nauczyciel)
    
    return nauczyciele

# Dodaj folder algotithm do ścieżki Pythona
sys.path.append(str(Path(__file__).parent))

from genetic_algorithm import AlgorytmGenetyczny
from individual import Individual

def main():
    try:
        klasy, plan, sale, przedmioty, nauczyciele = wczytaj_dane()
        
        print(f"Liczba nauczycieli: {len(nauczyciele)}")
        print(f"Liczba przedmiotów: {len(przedmioty)}")
        print(f"Liczba sal: {len(sale)}")
        print(f"Liczba klas: {len(klasy)}")
        print(f"Liczba slotów czasowych: {len(plan)}")
        
        # Testowe generowanie pojedynczego osobnika
        test_individual = Individual(klasy, plan, sale, przedmioty, nauczyciele)
        print(f"Fitness testowego osobnika: {test_individual.fitness}")
        
        # Policz zajęte sloty
        occupied_slots = 0
        for hour in test_individual.timetable:
            for day, slot in hour["Days"].items():
                if slot and slot.get("Subject"):
                    occupied_slots += 1
        
        print(f"Zajęte sloty w testowym osobniku: {occupied_slots}/{(len(plan) * 5)}")
        
        algorytm = AlgorytmGenetyczny(
            klasy=klasy,
            plan=plan,
            sale=sale,
            przedmioty=przedmioty,
            nauczyciele=nauczyciele,
            rozmiar_populacji=30,
            generacje=100,
            szansa_krzyzowania=0.8,
            szansa_mutacji=0.3,
            elityzm=3
        )
        
        najlepszy = algorytm.ewoluuj()
        najlepszy.zapisz_plany()
        
        # Sprawdź końcowy plan
        final_occupied = 0
        for hour in najlepszy.timetable:
            for day, slot in hour["Days"].items():
                if slot and slot.get("Subject"):
                    final_occupied += 1
        
        print(f"Końcowy plan: {final_occupied} zajętych slotów")
        print(f"Końcowy fitness: {najlepszy.fitness}")
        
        print("Pomyślnie zapisano plany:")
        print("- plany_klas.json")
        print("- plany_nauczycieli.json")
        print("- plany_sal.json")
        
    except Exception as e:
        print(f"Błąd: {str(e)}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    main()