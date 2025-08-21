import random
from copy import deepcopy
from individual import Individual
from fitness import FitnessCalculator
from crossover import krzyzowanie
from mutation import mutacja

class AlgorytmGenetyczny:
    def __init__(self, klasy, plan, sale, przedmioty, nauczyciele, 
                 rozmiar_populacji=100, generacje=500, 
                 szansa_krzyzowania=0.85, szansa_mutacji=0.15, elityzm=5):
        self.klasy = klasy
        self.plan = plan
        self.sale = sale
        self.przedmioty = przedmioty
        self.nauczyciele = nauczyciele
        self.rozmiar_populacji = rozmiar_populacji
        self.generacje = generacje
        self.szansa_krzyzowania = szansa_krzyzowania
        self.szansa_mutacji = szansa_mutacji
        self.elityzm = elityzm
        self.calculator = FitnessCalculator()
        
    def ewoluuj(self):
        populacja = self._inicjalizuj_populacje()
        najlepszy = None
        najlepsza_kara = float('inf')
        
        for generacja in range(self.generacje):
            populacja = self._ocen_populacje(populacja)
            
            if populacja[0].fitness < najlepsza_kara:
                najlepsza_kara = populacja[0].fitness
                najlepszy = deepcopy(populacja[0])
                print(f"Generacja {generacja}: Kara = {najlepsza_kara}")
            
            nowa_populacja = self._selekcja_elitarna(populacja)
            populacja = self._reprodukuj(nowa_populacja)
        
        return najlepszy or populacja[0]
    
    def _inicjalizuj_populacje(self):
        return [Individual(self.klasy, self.plan, self.sale, 
                         self.przedmioty, self.nauczyciele) 
                for _ in range(self.rozmiar_populacji)]
    
    def _ocen_populacje(self, populacja):
        for osobnik in populacja:
            self.calculator.calculate(osobnik)
        return sorted(populacja, key=lambda x: x.fitness)
    
    def _selekcja_elitarna(self, populacja):
        return populacja[:self.elityzm]
    
    def _reprodukuj(self, populacja):
        while len(populacja) < self.rozmiar_populacji:
            rodzic1, rodzic2 = self._wybierz_rodzicow(populacja)
            dziecko = self._krzyzuj(rodzic1, rodzic2)
            dziecko = self._mutuj(dziecko)
            populacja.append(dziecko)
        return populacja
    
    def _wybierz_rodzicow(self, populacja):
        # Turniej - wybierz 3 losowe osobniki i weź 2 najlepsze
        turniej = random.sample(populacja, min(3, len(populacja)))
        return sorted(turniej, key=lambda x: x.fitness)[:2]
    
    def _krzyzuj(self, rodzic1, rodzic2):
        if random.random() < self.szansa_krzyzowania:
            return krzyzowanie(rodzic1, rodzic2)
        return deepcopy(rodzic1)
    
    def _mutuj(self, osobnik):
        if random.random() < self.szansa_mutacji:
            return mutacja(osobnik, self.szansa_mutacji)
        return osobnik