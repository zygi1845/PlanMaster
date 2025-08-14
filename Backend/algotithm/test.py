import json
from genetic_algorithm import GeneticAlgorithm

# Wczytanie danych
with open('classes.json') as f:
    classes = json.load(f)
with open('hourplan.json') as f:
    schedule = json.load(f)
with open('rooms.json') as f:
    rooms = json.load(f)
with open('subjects.json') as f:
    subjects = json.load(f)
with open('teachers.json') as f:
    teachers = json.load(f)

# Uruchomienie algorytmu
ga = GeneticAlgorithm(classes, schedule, rooms, subjects, teachers)
best_schedule = ga.evolve()

# Zapisanie najlepszego planu
best_schedule.to_json('best_schedule.json')
print("Najlepszy plan lekcji został zapisany do pliku 'best_schedule.json'")