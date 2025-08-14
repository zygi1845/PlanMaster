import random
import json
from copy import deepcopy
from individual import Individual
from fitness import calculate_fitness
from crossover import crossover
from mutation import mutate

class GeneticAlgorithm:
    def __init__(self, classes, schedule, rooms, subjects, teachers, 
                 population_size=50, generations=100, crossover_rate=0.8, 
                 mutation_rate=0.1, elitism=2):
        self.classes = classes
        self.schedule = schedule
        self.rooms = rooms
        self.subjects = subjects
        self.teachers = teachers
        self.population_size = population_size
        self.generations = generations
        self.crossover_rate = crossover_rate
        self.mutation_rate = mutation_rate
        self.elitism = elitism
        
    def initialize_population(self):
        """Tworzy początkową populację"""
        return [Individual(self.classes, self.schedule, self.rooms, 
                         self.subjects, self.teachers) 
                for _ in range(self.population_size)]
    
    def evolve(self):
        """Główna pętla ewolucyjna"""
        population = self.initialize_population()
        
        for generation in range(self.generations):
            # Ocena populacji
            for individual in population:
                calculate_fitness(individual)
            
            # Sortowanie według przystosowania
            population.sort(key=lambda x: x.fitness)
            
            # Selekcja elitarna
            new_population = population[:self.elitism]
            
            # Selekcja turniejowa
            while len(new_population) < self.population_size:
                parent1 = min(random.sample(population, 5), key=lambda x: x.fitness)
                parent2 = min(random.sample(population, 5), key=lambda x: x.fitness)
                
                # Krzyżowanie
                if random.random() < self.crossover_rate:
                    child = crossover(parent1, parent2)
                else:
                    child = deepcopy(random.choice([parent1, parent2]))
                
                # Mutacja
                if random.random() < self.mutation_rate:
                    child = mutate(child, self.mutation_rate)
                
                new_population.append(child)
            
            population = new_population
            
            # Logowanie postępu
            if generation % 10 == 0:
                print(f"Generation {generation}, Best fitness: {population[0].fitness}")
        
        # Zwróć najlepszego osobnika
        population.sort(key=lambda x: x.fitness)
        return population[0]