from copy import deepcopy
import random

def cros(rodzic1, rodzic2):
    """Funkcja krzyżująca dwóch rodziców"""
    dziecko = deepcopy(rodzic1)
    punkt_krzyzowania = random.randint(0, len(dziecko.timetable)-1)
    
    for i in range(punkt_krzyzowania, len(dziecko.timetable)):
        dziecko.timetable[i] = deepcopy(rodzic2.timetable[i])
    
    return dziecko