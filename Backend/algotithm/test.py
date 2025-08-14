import json
import sys
from pathlib import Path

# Dodaj folder algotithm do ścieżki Pythona
sys.path.append(str(Path(__file__).parent))

from genetic_algorithm import AlgorytmGenetyczny

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
    
    with open(sciezki['classes']) as f:
        klasy = json.load(f)
    with open(sciezki['hourplan']) as f:
        plan = json.load(f)
    with open(sciezki['rooms']) as f:
        sale = json.load(f)
    with open(sciezki['subjects']) as f:
        przedmioty = json.load(f)
    with open(sciezki['teachers']) as f:
        nauczyciele = json.load(f)
        
    return klasy, plan, sale, przedmioty, nauczyciele

def main():
    try:
        klasy, plan, sale, przedmioty, nauczyciele = wczytaj_dane()
        
        algorytm = AlgorytmGenetyczny(
            klasy=klasy,
            plan=plan,
            sale=sale,
            przedmioty=przedmioty,
            nauczyciele=nauczyciele,
            rozmiar_populacji=100,
            generacje=200,
            szansa_krzyzowania=0.8,
            szansa_mutacji=0.2,
            elityzm=5
        )
        
        najlepszy = algorytm.ewoluuj()
        najlepszy.zapisz_do_json('najlepszy_plan.json')
        print(f"Znaleziono najlepszy plan z karą: {najlepszy.fitness}")
        
    except Exception as e:
        print(f"Błąd: {str(e)}")
        input("Naciśnij Enter aby zakończyć...")

if __name__ == "__main__":
    main()