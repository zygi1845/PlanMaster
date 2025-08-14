from copy import deepcopy
import random
def crossover(parent1, parent2):
    """Krzyżowanie jednopunktowe"""
    child = deepcopy(parent1)
    crossover_point = random.randint(0, len(child.timetable)-1)
    
    for i in range(crossover_point, len(child.timetable)):
        child.timetable[i] = deepcopy(parent2.timetable[i])
    
    return child