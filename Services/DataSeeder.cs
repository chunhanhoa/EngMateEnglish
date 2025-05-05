using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TiengAnh.Models;
using System.Text.Json;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace TiengAnh.Services
{
    public class DataSeeder
    {
        private readonly MongoDbService _mongoDbService;
        private readonly ILogger<DataSeeder> _logger;
        private readonly string _contentRootPath;
        private readonly IWebHostEnvironment _environment;

        public DataSeeder(MongoDbService mongoDbService, ILogger<DataSeeder> logger, IWebHostEnvironment environment)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
            _environment = environment;
            _contentRootPath = environment.ContentRootPath;
        }

        public async Task SeedDataAsync()
        {
            _logger.LogInformation("Starting to seed data...");
            
            await SeedTestsAsync();
            await SeedTopicsAsync();
            await SeedVocabulariesAsync();
            await SeedGrammarAsync();
            await SeedExercisesAsync();
            await SeedUsersAsync();
            await SeedProgressAsync();
            
            _logger.LogInformation("Data seeding completed.");
        }

        private async Task SeedTestsAsync()
        {
            try
            {
                var testCollection = _mongoDbService.GetCollection<TestModel>("Tests");
                
                // Check if collection is empty before seeding
                if (await testCollection.CountDocumentsAsync(FilterDefinition<TestModel>.Empty) == 0)
                {
                    string jsonPath = Path.Combine(_contentRootPath, "json", "test.json");
                    if (File.Exists(jsonPath))
                    {
                        string jsonContent = File.ReadAllText(jsonPath);
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var tests = JsonSerializer.Deserialize<List<TestModel>>(jsonContent, options);
                        
                        if (tests != null && tests.Any())
                        {
                            // Process each test to ensure proper ID mapping
                            foreach (var test in tests)
                            {
                                test.OnDeserialized();
                            }
                            
                            await testCollection.InsertManyAsync(tests);
                            _logger.LogInformation($"Successfully seeded {tests.Count} tests");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Test JSON file not found at {jsonPath}");
                        
                        // If JSON file not found, seed with basic test data
                        await SeedBasicTestDataAsync(testCollection);
                    }
                }
                else
                {
                    _logger.LogInformation("Tests collection already has data. Skipping seeding.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error seeding test data: {ex.Message}");
            }
        }
        
        private async Task SeedBasicTestDataAsync(IMongoCollection<TestModel> testCollection)
        {
            // Create some basic test data
            var tests = new List<TestModel>
            {
                new TestModel
                {
                    TestIdentifier = "test_001",
                    Title = "Basic English Grammar Test",
                    Description = "Test your basic English grammar knowledge",
                    Duration = 30,
                    Level = "A1",
                    CreatedDate = DateTime.Now.AddDays(-10),
                    UpdatedDate = DateTime.Now.AddDays(-10),
                    Category = "Grammar",
                    ImageUrl = "/images/tests/test-a1.jpg",
                    Questions = new List<TestQuestionModel>
                    {
                        new TestQuestionModel
                        {
                            QuestionId = 1,
                            QuestionText = "What is your name?",
                            Options = new List<string> { "My name is John", "I am fine", "I am 25 years old", "I am from London" },
                            CorrectAnswer = 0
                        },
                        new TestQuestionModel
                        {
                            QuestionId = 2,
                            QuestionText = "She ___ a student.",
                            Options = new List<string> { "am", "is", "are", "be" },
                            CorrectAnswer = 1
                        }
                    }
                }
            };
            
            // Make sure each test has proper ID mapping
            foreach (var test in tests)
            {
                test.OnDeserialized();
            }
            
            await testCollection.InsertManyAsync(tests);
            _logger.LogInformation("Seeded basic test data");
        }

        private async Task SeedTopicsAsync()
        {
            _logger.LogInformation("Checking Topics collection");
            var topicCollection = _mongoDbService.GetCollection<TopicModel>("Topics");
            
            long count = await topicCollection.CountDocumentsAsync(FilterDefinition<TopicModel>.Empty);
            _logger.LogInformation($"Found {count} existing topics");
            
            if (count == 0)
            {
                _logger.LogInformation("Seeding Topics collection");
                await topicCollection.InsertManyAsync(new List<TopicModel>
                {
                    new TopicModel
                    {
                        ID_CD = 1,
                        Name_CD = "Animals",
                        Description_CD = "Từ vựng về các loài động vật",
                        IconPath = "/images/topics/animals.png",
                        Image_CD = "/images/topics/animals-bg.jpg",
                        Level = "A1",
                        TotalItems = 50,
                        TotalWords = 50,
                        WordCount = 50,
                        BackgroundColor = "#8BC34A",
                        Type_CD = "Vocabulary"
                    },
                    new TopicModel
                    {
                        ID_CD = 2,
                        Name_CD = "Food",
                        Description_CD = "Từ vựng về thực phẩm và đồ ăn",
                        IconPath = "/images/topics/food.png",
                        Image_CD = "/images/topics/food-bg.jpg",
                        Level = "A1",
                        TotalItems = 45,
                        TotalWords = 45,
                        WordCount = 45,
                        BackgroundColor = "#FF9800",
                        Type_CD = "Vocabulary"
                    },
                    new TopicModel
                    {
                        ID_CD = 3,
                        Name_CD = "Travel",
                        Description_CD = "Từ vựng về du lịch và đi lại",
                        IconPath = "/images/topics/travel.png",
                        Image_CD = "/images/topics/travel-bg.jpg",
                        Level = "A2",
                        TotalItems = 40,
                        TotalWords = 40,
                        WordCount = 40,
                        BackgroundColor = "#2196F3",
                        Type_CD = "Vocabulary"
                    }
                });
                _logger.LogInformation("Topics seeded successfully");
            }
            else
            {
                // Cập nhật Type_CD cho các topic hiện có nếu chưa có
                var updateDefinition = Builders<TopicModel>.Update.Set(t => t.Type_CD, "Vocabulary");
                await topicCollection.UpdateManyAsync(t => string.IsNullOrEmpty(t.Type_CD), updateDefinition);
            }
        }
        
        private async Task SeedVocabulariesAsync()
        {
            _logger.LogInformation("Checking Vocabularies collection");
            var vocabularyCollection = _mongoDbService.GetCollection<VocabularyModel>("Vocabularies");
            
            long count = await vocabularyCollection.CountDocumentsAsync(FilterDefinition<VocabularyModel>.Empty);
            _logger.LogInformation($"Found {count} existing vocabularies");
            
            if (count == 0)
            {
                _logger.LogInformation("Seeding Vocabularies collection");
                await vocabularyCollection.InsertManyAsync(new List<VocabularyModel>
                {
                    new VocabularyModel 
                    { 
                        ID_TV = 1, 
                        Word_TV = "Cat", 
                        Meaning_TV = "Con mèo", 
                        Example_TV = "I have a black cat as a pet.", 
                        Audio_TV = "/audio/cat.mp3", 
                        Image_TV = "/images/vocabulary/cat.jpg", 
                        Level_TV = "A1", 
                        ID_CD = 1, 
                        ID_LT = "n", 
                        PartOfSpeech = "noun", 
                        TopicName = "Animals",
                        IsFavorite = false
                    },
                    new VocabularyModel 
                    { 
                        ID_TV = 2, 
                        Word_TV = "Dog", 
                        Meaning_TV = "Con chó", 
                        Example_TV = "My dog likes to play in the garden.", 
                        Audio_TV = "/audio/dog.mp3", 
                        Image_TV = "/images/vocabulary/dog.jpg", 
                        Level_TV = "A1", 
                        ID_CD = 1, 
                        ID_LT = "n", 
                        PartOfSpeech = "noun", 
                        TopicName = "Animals",
                        IsFavorite = true
                    },
                    new VocabularyModel 
                    { 
                        ID_TV = 3, 
                        Word_TV = "Elephant", 
                        Meaning_TV = "Con voi", 
                        Example_TV = "Elephants are the largest land animals.", 
                        Audio_TV = "/audio/elephant.mp3", 
                        Image_TV = "/images/vocabulary/elephant.jpg", 
                        Level_TV = "A1", 
                        ID_CD = 1, 
                        ID_LT = "n", 
                        PartOfSpeech = "noun", 
                        TopicName = "Animals",
                        IsFavorite = false
                    },
                    new VocabularyModel 
                    { 
                        ID_TV = 4, 
                        Word_TV = "Apple", 
                        Meaning_TV = "Quả táo", 
                        Example_TV = "I eat an apple every day.", 
                        Audio_TV = "/audio/apple.mp3", 
                        Image_TV = "/images/vocabulary/apple.jpg", 
                        Level_TV = "A1", 
                        ID_CD = 2, 
                        ID_LT = "n", 
                        PartOfSpeech = "noun", 
                        TopicName = "Food & Drinks",
                        IsFavorite = true
                    },
                    new VocabularyModel 
                    { 
                        ID_TV = 5, 
                        Word_TV = "Pencil", 
                        Meaning_TV = "Bút chì", 
                        Example_TV = "I write with a pencil.", 
                        Audio_TV = "/audio/pencil.mp3", 
                        Image_TV = "/images/vocabulary/pencil.jpg", 
                        Level_TV = "A1", 
                        ID_CD = 3, 
                        ID_LT = "n", 
                        PartOfSpeech = "noun", 
                        TopicName = "School",
                        IsFavorite = false
                    }
                });
                _logger.LogInformation("Vocabularies seeded successfully");
            }
        }
        
        private async Task SeedGrammarAsync()
        {
            var grammarCollection = _mongoDbService.GetCollection<GrammarModel>("Grammar");
            
            if (await grammarCollection.CountDocumentsAsync(FilterDefinition<GrammarModel>.Empty) == 0)
            {
                await grammarCollection.InsertManyAsync(new List<GrammarModel>
                {
                    new GrammarModel
                    {
                        ID_NP = 1,
                        Title_NP = "Simple Present Tense",
                        Description_NP = "Học cách diễn đạt thói quen, sự thật và hoạt động thường xuyên bằng thì hiện tại đơn",
                        Content_NP = "<h2>Thì hiện tại đơn (Simple Present Tense)</h2><p>Thì hiện tại đơn được dùng để diễn tả:</p><ul><li>Một sự thật, chân lý khách quan</li><li>Một thói quen, hành động lặp đi lặp lại ở hiện tại</li><li>Một khả năng, một tình trạng ở hiện tại</li><li>Lịch trình, thời gian biểu đã định sẵn</li></ul><h3>Cấu trúc:</h3><p><b>Khẳng định:</b></p><ul><li>I/You/We/They + V(nguyên thể)</li><li>He/She/It + V(nguyên thể) + s/es</li></ul><p><b>Phủ định:</b></p><ul><li>I/You/We/They + do not (don't) + V(nguyên thể)</li><li>He/She/It + does not (doesn't) + V(nguyên thể)</li></ul><p><b>Nghi vấn:</b></p><ul><li>Do + I/you/we/they + V(nguyên thể)?</li><li>Does + he/she/it + V(nguyên thể)?</li></ul><h3>Dấu hiệu nhận biết:</h3><p>Thường đi với các trạng từ chỉ tần suất như: always, usually, often, sometimes, rarely, never, every day/week/month/year...</p>",
                        TimeUpload_NP = DateTime.Now.AddDays(-30),
                        ID_CD = 1,
                        TopicName = "Basic Grammar",
                        Level = "A1",
                        Examples = new List<string> { "I go to school every day.", "She doesn't like coffee.", "Do they play football on weekends?" },
                        ProgressPercentage = 80,
                        IsFavorite = false
                    },
                    new GrammarModel
                    {
                        ID_NP = 2,
                        Title_NP = "Present Continuous Tense",
                        Description_NP = "Hiểu và sử dụng thì hiện tại tiếp diễn để diễn đạt hành động đang diễn ra",
                        Content_NP = "<h2>Thì hiện tại tiếp diễn (Present Continuous Tense)</h2><p>Thì hiện tại tiếp diễn được dùng để diễn tả:</p><ul><li>Một hành động đang diễn ra tại thời điểm nói</li><li>Một hành động đang diễn ra trong giai đoạn hiện tại, không nhất thiết ở thời điểm nói</li><li>Một hành động đã được sắp xếp, lên kế hoạch trong tương lai gần</li><li>Diễn tả sự phàn nàn với always, constantly</li></ul><h3>Cấu trúc:</h3><p><b>Khẳng định:</b></p><ul><li>I am (I'm) + V-ing</li><li>You/We/They are (You're/We're/They're) + V-ing</li><li>He/She/It is (He's/She's/It's) + V-ing</li></ul><p><b>Phủ định:</b></p><ul><li>I am not (I'm not) + V-ing</li><li>You/We/They are not (aren't) + V-ing</li><li>He/She/It is not (isn't) + V-ing</li></ul><p><b>Nghi vấn:</b></p><ul><li>Am I + V-ing?</li><li>Are you/we/they + V-ing?</li><li>Is he/she/it + V-ing?</li></ul><h3>Dấu hiệu nhận biết:</h3><p>Thường đi với các trạng từ chỉ thời gian như: now, at the moment, at present, currently, right now...</p>",
                        TimeUpload_NP = DateTime.Now.AddDays(-25),
                        ID_CD = 1,
                        TopicName = "Basic Grammar",
                        Level = "A1",
                        Examples = new List<string> { "I am studying English now.", "She isn't watching TV right now.", "Are they working on the project?" },
                        ProgressPercentage = 60,
                        IsFavorite = true
                    },
                    new GrammarModel
                    {
                        ID_NP = 3,
                        Title_NP = "Past Simple Tense",
                        Description_NP = "Học cách diễn đạt hành động đã xảy ra trong quá khứ bằng thì quá khứ đơn",
                        Content_NP = "<h2>Thì quá khứ đơn (Past Simple Tense)</h2><p>Thì quá khứ đơn được dùng để diễn tả:</p><ul><li>Một hành động đã hoàn thành trong quá khứ</li><li>Một thói quen, hành động lặp đi lặp lại trong quá khứ</li><li>Một chuỗi hành động xảy ra liên tiếp trong quá khứ</li><li>Một tình trạng, trạng thái tồn tại trong quá khứ</li></ul><h3>Cấu trúc:</h3><p><b>Khẳng định:</b></p><ul><li>I/You/He/She/It/We/They + V-ed hoặc V2 (với động từ bất quy tắc)</li></ul><p><b>Phủ định:</b></p><ul><li>I/You/He/She/It/We/They + did not (didn't) + V (nguyên thể)</li></ul><p><b>Nghi vấn:</b></p><ul><li>Did + I/you/he/she/it/we/they + V (nguyên thể)?</li></ul><h3>Dấu hiệu nhận biết:</h3><p>Thường đi với các trạng từ chỉ thời gian quá khứ như: yesterday, last week/month/year, ... ago, in 1990, when I was young...</p>",
                        TimeUpload_NP = DateTime.Now.AddDays(-20),
                        ID_CD = 2,
                        TopicName = "Basic Grammar",
                        Level = "A2",
                        Examples = new List<string> { "I visited my grandparents last weekend.", "She didn't go to school yesterday.", "Did they finish the project?" },
                        ProgressPercentage = 40,
                        IsFavorite = true
                    }
                });
                
                _logger.LogInformation("Grammar data seeded successfully");
            }
        }
        
        private async Task SeedExercisesAsync()
        {
            var exerciseCollection = _mongoDbService.GetCollection<ExerciseModel>("Exercises");
            
            if (await exerciseCollection.CountDocumentsAsync(FilterDefinition<ExerciseModel>.Empty) == 0)
            {
                await exerciseCollection.InsertManyAsync(new List<ExerciseModel>
                {
                    new ExerciseModel
                    {
                        ID_BT = 1,
                        Question = "What color is the sky?",
                        Question_BT = "What color is the sky?",
                        CorrectAnswer = "Blue",
                        Answer_BT = "Blue",
                        Options = new List<string> { "Red", "Blue", "Green", "Yellow" },
                        Option_A = "Red",
                        Option_B = "Blue",
                        Option_C = "Green",
                        Option_D = "Yellow",
                        ExerciseType = "MultipleChoice",
                        Level = "A1",
                        ID_CD = 1,
                        Explanation = "The sky appears blue because of the way sunlight is scattered by the atmosphere.",
                        Explanation_BT = "The sky appears blue because of the way sunlight is scattered by the atmosphere."
                    },
                    new ExerciseModel
                    {
                        ID_BT = 2,
                        Question = "She ___ to school every day.",
                        CorrectAnswer = "goes",
                        Options = new List<string> { "go", "goes", "going", "went" },
                        ExerciseType = "FillBlank",
                        Level = "A1",
                        ID_CD = 1,
                        Explanation = "With third person singular (she), we add -s or -es to the base form of the verb."
                    },
                    new ExerciseModel
                    {
                        ID_BT = 3,
                        Question = "to school I go every day",
                        CorrectAnswer = "I go to school every day",
                        ExerciseType = "WordOrdering",
                        Level = "A1",
                        ID_CD = 1,
                        Explanation = "The correct word order in English is typically Subject-Verb-Object."
                    },
                    new ExerciseModel 
                    { 
                        ID_BT = 4,
                        Question = "The cat ___ on the sofa.",
                        CorrectAnswer = "is",
                        Options = new List<string> { "is", "are", "am", "be" },
                        ExerciseType = "FillBlank",
                        Level = "A1",
                        ID_CD = 1,
                        Explanation = "Với chủ ngữ là 'The cat' (số ít), ta dùng 'is'.",
                        TopicName = "Animals"
                    },
                    new ExerciseModel 
                    { 
                        ID_BT = 5,
                        Question = "Put the words in the correct order: 'cat, black, a, is, it'",
                        CorrectAnswer = "It is a black cat",
                        Options = new List<string> { "It", "is", "a", "black", "cat" },
                        ExerciseType = "WordOrdering",
                        Level = "A1",
                        ID_CD = 1,
                        Explanation = "Thứ tự đúng: subject + verb + article + adjective + noun.",
                        TopicName = "Animals"
                    }
                });
            }
        }
        
        private async Task SeedUsersAsync()
        {
            var userCollection = _mongoDbService.GetCollection<UserModel>("Users");
            
            if (await userCollection.CountDocumentsAsync(FilterDefinition<UserModel>.Empty) == 0)
            {
                await userCollection.InsertManyAsync(new List<UserModel>
                {
                    new UserModel
                    {
                        UserId = "user123",
                        UserName = "hocsinh001",
                        Email = "demo@example.com",
                        PasswordHash = "Password123!", // Trong thực tế phải hash password
                        FullName = "Nguyễn Văn A",
                        Avatar = "/images/avatar/default.jpg",
                        Level = "A2",
                        RegisterDate = DateTime.Now.AddMonths(-2),
                        Points = 450,
                        Roles = new List<string> { "Student" }
                    },
                    new UserModel
                    {
                        UserId = "admin123",
                        UserName = "admin001",
                        Email = "admin@example.com",
                        PasswordHash = "Admin123!", // Trong thực tế phải hash password
                        FullName = "Trần Thị B",
                        Avatar = "/images/avatar/admin.jpg",
                        Level = "C1",
                        RegisterDate = DateTime.Now.AddYears(-1),
                        Points = 1200,
                        Roles = new List<string> { "Admin", "Teacher" }
                    }
                });
            }
        }
        
        private async Task SeedProgressAsync()
        {
            var progressCollection = _mongoDbService.GetCollection<ProgressModel>("Progress");
            
            if (await progressCollection.CountDocumentsAsync(FilterDefinition<ProgressModel>.Empty) == 0)
            {
                await progressCollection.InsertManyAsync(new List<ProgressModel>
                {
                    new ProgressModel
                    {
                        UserId = "user123",
                        VocabularyProgress = 65,
                        GrammarProgress = 40,
                        ExerciseProgress = 30,
                        TotalPoints = 750,
                        Level = "A2",
                        LastCompletedItems = new List<LastCompletedItemModel>
                        {
                            new LastCompletedItemModel 
                            { 
                                Id = 1, 
                                Type = "Vocabulary", 
                                Title = "Animals", 
                                CompletedDate = DateTime.Now.AddDays(-1),
                                Score = 85
                            },
                            new LastCompletedItemModel 
                            { 
                                Id = 2, 
                                Type = "Grammar", 
                                Title = "Simple Present Tense", 
                                CompletedDate = DateTime.Now.AddDays(-3),
                                Score = 75
                            },
                            new LastCompletedItemModel 
                            { 
                                Id = 3, 
                                Type = "Exercise", 
                                Title = "Vocabulary Exercise", 
                                CompletedDate = DateTime.Now.AddDays(-5),
                                Score = 90
                            }
                        },
                        CompletedTopics = new List<CompletedTopicModel>
                        {
                            new CompletedTopicModel { TopicId = 1, TopicName = "Animals", CompletionPercentage = 100 },
                            new CompletedTopicModel { TopicId = 2, TopicName = "Food & Drinks", CompletionPercentage = 80 },
                            new CompletedTopicModel { TopicId = 3, TopicName = "School", CompletionPercentage = 60 },
                            new CompletedTopicModel { TopicId = 4, TopicName = "Family", CompletionPercentage = 40 },
                            new CompletedTopicModel { TopicId = 5, TopicName = "Sports", CompletionPercentage = 20 }
                        }
                    }
                });
            }
        }
    }
}
