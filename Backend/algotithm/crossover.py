"""Crossover operators"""
import random
from typing import Tuple
from individual import Individual


class CrossoverOperator:
    """Crossover operator"""
    
    def __init__(self, crossover_rate: float = 0.8):
        self.crossover_rate = crossover_rate
    
    def crossover(self, parent1: Individual, parent2: Individual) -> Tuple[Individual, Individual]:
        """Single point crossover"""
        if random.random() > self.crossover_rate or not parent1.lessons or not parent2.lessons:
            return parent1.copy(), parent2.copy()
        
        child1 = Individual()
        child2 = Individual()
        
        split = len(parent1.lessons) // 2
        
        child1.lessons = [l.copy() for l in parent1.lessons[:split] + parent2.lessons[split:]]
        child2.lessons = [l.copy() for l in parent2.lessons[:split] + parent1.lessons[split:]]
        
        return child1, child2

# from copy import deepcopy
# import random

# def cros(rodzic1, rodzic2):
#     """Funkcja krzyżująca dwóch rodziców"""
#     dziecko = deepcopy(rodzic1)
#     punkt_krzyzowania = random.randint(0, len(dziecko.timetable)-1)
    
#     for i in range(punkt_krzyzowania, len(dziecko.timetable)):
#         dziecko.timetable[i] = deepcopy(rodzic2.timetable[i])
    
#     return dziecko