"""Genetic algorithm main module"""
import random
import json
import os
from typing import List, Dict, Tuple
from individual import Individual, Lesson
from fitness import FitnessCalculator
from crossover import CrossoverOperator
from mutation import MutationOperator


def clean_csharp_json(obj):
    """Remove $id, $values, $ref from C# JSON"""
    if isinstance(obj, dict):
        if '$values' in obj:
            return clean_csharp_json(obj['$values'])
        cleaned = {}
        for key, value in obj.items():
            if key not in ['$id', '$ref']:
                cleaned[key] = clean_csharp_json(value)
        return cleaned
    elif isinstance(obj, list):
        return [clean_csharp_json(item) for item in obj]
    else:
        return obj


class GeneticAlgorithm:
    """Genetic Algorithm"""
    
    def __init__(self, subjects, teachers, rooms, classes, hourplan, weights,
                 population_size=50, generations=1000, mutation_rate=0.2, crossover_rate=0.8):
        
        self.subjects = clean_csharp_json(subjects)
        self.teachers = clean_csharp_json(teachers)
        self.rooms = clean_csharp_json(rooms)
        self.classes = clean_csharp_json(classes)
        self.hourplan = clean_csharp_json(hourplan)
        self.weights = clean_csharp_json(weights)
        
        self.population_size = population_size
        self.generations = generations
        
        self._prepare_data()
        
        self.fitness_calc = FitnessCalculator(self.constraints)
        self.crossover_op = CrossoverOperator(crossover_rate)
        self.mutation_op = MutationOperator(mutation_rate, self.teachers_dict, 
                                           self.rooms, self.subjects, self.hours_per_day)
    
    def _prepare_data(self):
        """Prepare data structures"""
        self.teachers_dict = {t['Nr_pracownika']: t for t in self.teachers}
        
        self.class_types = []
        self.class_type_counts = {}
        
        for cls in self.classes:
            class_type = cls['NazwaTypu']
            count = cls['Ilosc_klas_typu']
            
            for i in range(count):
                if count > 1:
                    numbered_name = f"{class_type} {i + 1}"
                else:
                    numbered_name = class_type
                
                self.class_types.append(numbered_name)
                self.class_type_counts[numbered_name] = class_type
        
        self.class_teachers = {}
        for class_name in self.class_types:
            base_type = self.class_type_counts.get(class_name, class_name)
            
            self.class_teachers[class_name] = []
            for teacher in self.teachers:
                if teacher.get('SelectedSubject'):
                    if base_type in teacher['SelectedSubject'].get('ClassTypes', []):
                        self.class_teachers[class_name].append(teacher)
        
        self.hours_per_day = len(self.hourplan)
        
        self.constraints = {
            'teachers_dict': self.teachers_dict,
            'class_types': self.class_types,
            'class_type_counts': self.class_type_counts,
            'class_teachers': self.class_teachers,
            'rooms': self.rooms,
            'subjects': self.subjects,
            'weights': self.weights,
            'hours_per_day': self.hours_per_day
        }
    
    def _create_random_individual(self) -> Individual:
        """Create random individual"""
        individual = Individual()
        
        for class_type in self.class_types:
            teachers = self.class_teachers.get(class_type, [])
            
            for teacher in teachers:
                subject = teacher.get('SelectedSubject')
                base_type = self.class_type_counts.get(class_type, class_type)
                
                if not subject or base_type not in subject.get('ClassTypes', []):
                    continue
                
                hours_needed = subject['Hours']
                hours_assigned = 0
                attempts = 0
                
                while hours_assigned < hours_needed and attempts < 1000:
                    attempts += 1
                    day = random.randint(0, 5)
                    hour = random.randint(0, self.hours_per_day - 1)
                    
                    if day >= len(teacher['Availability']) or hour >= len(teacher['Availability'][day]):
                        continue
                    if not teacher['Availability'][day][hour]:
                        continue
                    
                    suitable_rooms = [r for r in self.rooms if r['RoomType'] == subject['SubjectType']]
                    rooms = suitable_rooms if suitable_rooms else self.rooms
                    if not rooms:
                        continue
                    
                    room = random.choice(rooms)
                    
                    individual.add_lesson(Lesson(
                        subject_name=subject['Name'],
                        teacher_id=teacher['Nr_pracownika'],
                        teacher_name=teacher['Nazwisko'],
                        class_type=class_type,
                        room_number=room['RoomNumber'],
                        day=day,
                        hour=hour
                    ))
                    hours_assigned += 1
        
        return individual
    
    def _initialize_population(self) -> List[Individual]:
        """Initialize population"""
        population = []
        for _ in range(self.population_size):
            individual = self._create_random_individual()
            self.fitness_calc.calculate(individual)
            population.append(individual)
        return population
    
    def _selection(self, population: List[Individual]) -> List[Individual]:
        """Tournament selection"""
        selected = []
        for _ in range(len(population)):
            tournament = random.sample(population, 3)
            selected.append(max(tournament, key=lambda i: i.fitness))
        return selected
    
    def _create_next_generation(self, population: List[Individual]) -> List[Individual]:
        """Create next generation"""
        selected = self._selection(population)
        new_pop = [max(population, key=lambda i: i.fitness).copy()]
        
        while len(new_pop) < self.population_size:
            p1, p2 = random.sample(selected, 2)
            c1, c2 = self.crossover_op.crossover(p1, p2)
            
            self.mutation_op.mutate(c1)
            self.mutation_op.mutate(c2)
            
            self.fitness_calc.calculate(c1)
            self.fitness_calc.calculate(c2)
            
            new_pop.extend([c1, c2])
        
        return new_pop[:self.population_size]
    
    def run(self, verbose=True) -> Individual:
        """Run genetic algorithm"""
        if verbose:
            print("=" * 60)
            print("GENETIC ALGORITHM START")
            print("=" * 60)
        
        population = self._initialize_population()
        best = max(population, key=lambda i: i.fitness)
        
        if verbose:
            print(f"Initial best fitness: {best.fitness:.2f}")
        
        for gen in range(self.generations):
            population = self._create_next_generation(population)
            current_best = max(population, key=lambda i: i.fitness)
            
            if current_best.fitness > best.fitness:
                best = current_best.copy()
                if verbose:
                    print(f"Gen {gen}: NEW BEST fitness = {current_best.fitness:.2f}")
            
            if verbose and gen % 100 == 0:
                avg = sum(i.fitness for i in population) / len(population)
                print(f"Gen {gen}: best = {current_best.fitness:.2f}, avg = {avg:.2f}")
        
        if verbose:
            print(f"\nFinal best fitness: {best.fitness:.2f}")
            print(f"Total lessons: {len(best.lessons)}")
            print("=" * 60)
        
        return best
    
    def export_schedules(self, individual: Individual) -> Tuple[Dict, Dict, Dict]:
        """Export schedules to JSON format"""
        class_schedules = {}
        for ct in self.class_types:
            schedule = [[[] for _ in range(self.hours_per_day)] for _ in range(6)]
            for lesson in individual.lessons:
                if lesson.class_type == ct:
                    schedule[lesson.day][lesson.hour].append({
                        'subject': lesson.subject_name,
                        'teacher': lesson.teacher_name,
                        'room': lesson.room_number
                    })
            class_schedules[ct] = schedule
        
        teacher_schedules = {}
        for teacher in self.teachers:
            tid = teacher['Nr_pracownika']
            tname = teacher['Nazwisko']
            schedule = [[[] for _ in range(self.hours_per_day)] for _ in range(6)]
            for lesson in individual.lessons:
                if lesson.teacher_id == tid:
                    schedule[lesson.day][lesson.hour].append({
                        'subject': lesson.subject_name,
                        'class': lesson.class_type,
                        'room': lesson.room_number
                    })
            teacher_schedules[tname] = schedule
        
        room_schedules = {}
        for room in self.rooms:
            rnum = room['RoomNumber']
            schedule = [[[] for _ in range(self.hours_per_day)] for _ in range(6)]
            for lesson in individual.lessons:
                if lesson.room_number == rnum:
                    schedule[lesson.day][lesson.hour].append({
                        'subject': lesson.subject_name,
                        'teacher': lesson.teacher_name,
                        'class': lesson.class_type
                    })
            room_schedules[rnum] = schedule
        
        return class_schedules, teacher_schedules, room_schedules


def load_json(filepath: str):
    """Load JSON file"""
    with open(filepath, 'r', encoding='utf-8') as f:
        return json.load(f)


def save_json(data, filepath: str):
    """Save JSON file"""
    os.makedirs(os.path.dirname(filepath), exist_ok=True)
    with open(filepath, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)