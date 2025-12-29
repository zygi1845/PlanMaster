"""Main script to run genetic algorithm"""
from genetic_algorithm import GeneticAlgorithm, load_json, save_json
import os

# Pliki JSON są w Inzynierka/ (2 poziomy wyżej)
print("Loading data...")
subjects = load_json('../../subjects.json')
teachers = load_json('../../teachers.json')
rooms = load_json('../../rooms.json')
classes = load_json('../../class.json')
hourplan = load_json('../../hourplan.json')
weights = load_json('../../weights_config.json')

print("Creating genetic algorithm...")
ga = GeneticAlgorithm(
    subjects=subjects,
    teachers=teachers,
    rooms=rooms,
    classes=classes,
    hourplan=hourplan,
    weights=weights,
    population_size=50,
    generations=500,
    mutation_rate=0.2,
    crossover_rate=0.8
)

print("Running algorithm...\n")
best = ga.run(verbose=True)

print("\nExporting schedules...")
class_sched, teacher_sched, room_sched = ga.export_schedules(best)

save_json(class_sched, 'plany/class_schedules.json')
save_json(teacher_sched, 'plany/teacher_schedules.json')
save_json(room_sched, 'plany/room_schedules.json')

print("\n✓ Schedules saved to plany/ folder")
print("  - class_schedules.json")
print("  - teacher_schedules.json")
print("  - room_schedules.json")