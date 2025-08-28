import random
from copy import deepcopy
from individual import Individual
from fitness import FitnessCalculator
from crossover import cros
from mutation import mutat

class AlgorytmGenetyczny:
    def __init__(self, klasy, plan, sale, przedmioty, nauczyciele, 
                 rozmiar_populacji=100, generacje=500, 
                 szansa_krzyzowania=0.85, szansa_mutacji=0.15, elityzm=5):
        self.klasy = klasy  # To są pojedyncze klasy
        self.plan = plan
        self.sale = sale
        self.przedmioty = przedmioty
        self.nauczyciele = nauczyciele
        self.rozmiar_populacji = rozmiar_populacji
        self.generacje = generacje
        self.szansa_krzyzowania = szansa_krzyzowania
        self.szansa_mutacji = szansa_mutacji
        self.elityzm = elityzm

        # Tworzymy populację
        self.population = [Individual(klasy, plan, sale, przedmioty, nauczyciele)
                           for _ in range(self.rozmiar_populacji)]

        # Kalkulator fitness - przekazujemy typy klas (musimy je wyodrębnić)
        # Wyodrębniamy unikalne typy klas
        typy_klas = []
        seen_types = set()
        for klasa in klasy:
            class_type = klasa.get("NazwaTypu") or klasa.get("Typ") or klasa.get("Name") or "UNKNOWN"
            if class_type not in seen_types:
                seen_types.add(class_type)
                typy_klas.append({"NazwaTypu": class_type})
        
        self.calculator = FitnessCalculator(
            subjects=self.przedmioty,
            teachers=self.nauczyciele,
            rooms=self.sale,
            classes=typy_klas  # Przekazujemy typy klas, nie pojedyncze klasy
        )

        # Liczymy fitness dla całej populacji
        self.calculator.calculate_population(self.population)

    def ewoluuj(self):
        populacja = deepcopy(self.population)
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

    def _ocen_populacje(self, populacja):
        self.calculator.calculate_population(populacja)
        return sorted(populacja, key=lambda x: x.fitness)

    def _selekcja_elitarna(self, populacja):
        return populacja[:self.elityzm]

    def _reprodukuj(self, populacja):
        nowa_populacja = populacja[:]

        while len(nowa_populacja) < self.rozmiar_populacji:
            rodzic1, rodzic2 = self._wybierz_rodzicow(populacja)

            if random.random() < self.szansa_krzyzowania:
                dziecko = cros(rodzic1, rodzic2)
            else:
                dziecko = deepcopy(random.choice([rodzic1, rodzic2]))

            if random.random() < self.szansa_mutacji:
                dziecko = mutat(dziecko, self.szansa_mutacji)

            self.calculator.calculate_individual(dziecko)
            nowa_populacja.append(dziecko)

        return sorted(nowa_populacja, key=lambda x: x.fitness)[:self.rozmiar_populacji]

    def _wybierz_rodzicow(self, populacja):
        turniej = random.sample(populacja, min(5, len(populacja)))
        return sorted(turniej, key=lambda x: x.fitness)[:2]