using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Driver;
using TiengAnh.Models;
using TiengAnh.Services;

namespace TiengAnh.Services
{
    public class DataImportService
    {
        private readonly MongoDbService _mongoDbService;

        public DataImportService(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        public async Task ImportExercisesFromJson(string filePath)
        {
            try
            {
                // Read the JSON file
                string jsonData = await File.ReadAllTextAsync(filePath);
                
                // Deserialize JSON to a list of exercises
                var exercises = JsonSerializer.Deserialize<List<ExerciseModel>>(
                    jsonData, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                
                if (exercises == null || !exercises.Any())
                {
                    Console.WriteLine("No exercises found in JSON file.");
                    return;
                }
                
                // Get the exercises collection
                var collection = _mongoDbService.GetCollection<ExerciseModel>("Exercises");
                
                // Check if collection is empty before importing
                var count = await collection.CountDocumentsAsync(FilterDefinition<ExerciseModel>.Empty);
                if (count > 0)
                {
                    Console.WriteLine("Exercises collection is not empty. Skipping import.");
                    return;
                }
                
                // Insert all exercises
                await collection.InsertManyAsync(exercises);
                Console.WriteLine($"Successfully imported {exercises.Count} exercises.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing exercises: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
