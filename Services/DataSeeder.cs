using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TiengAnh.Models;

namespace TiengAnh.Services
{
    public class DataSeeder
    {
        private readonly ILogger<DataSeeder> _logger;
        private readonly MongoDbService _mongoDbService;

        public DataSeeder(ILogger<DataSeeder> logger, MongoDbService mongoDbService)
        {
            _logger = logger;
            _mongoDbService = mongoDbService;
        }

        public async Task SeedDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting data seeding process");
                
                await SeedTopicsAsync();
                await SeedVocabulariesAsync();
                await SeedGrammarAsync();
                await SeedExercisesAsync();
                await SeedUsersAsync();
                await SeedProgressAsync();
                
                _logger.LogInformation("Data seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during data seeding");
            }
        }

        private async Task SeedTestsAsync()
        {
            var testCollection = _mongoDbService.GetCollection<TestModel>("Tests");
            
            // Kiểm tra nếu collection rỗng thì mới thêm dữ liệu
            if (await testCollection.CountDocumentsAsync(FilterDefinition<TestModel>.Empty) == 0)
            {
                await testCollection.InsertManyAsync(new List<TestModel>
                {
                    new TestModel
                    {
                        TestID = 1,
                        Title = "Bài kiểm tra trình độ A1",
                        Description = "Bài kiểm tra đánh giá trình độ cơ bản A1",
                        Duration = 30,
                        TotalQuestions = 20,
                        Level = "A1",
                        CreatedDate = DateTime.Now.AddDays(-10),
                        Category = "Trình độ",
                        ImageUrl = "/images/tests/test-a1.jpg",
                        Questions = new List<TestQuestionModel>
                        {
                            new TestQuestionModel
                            {
                                QuestionID = 1,
                                QuestionText = "What is the capital of England?",
                                Options = new List<string> { "Paris", "London", "Berlin", "Madrid" },
                                CorrectAnswer = 1
                            },
                            new TestQuestionModel
                            {
                                QuestionID = 2,
                                QuestionText = "How many days are there in a week?",
                                Options = new List<string> { "5", "6", "7", "8" },
                                CorrectAnswer = 2
                            }
                        }
                    },
                    new TestModel
                    {
                        TestID = 2,
                        Title = "Bài kiểm tra trình độ A2",
                        Description = "Bài kiểm tra đánh giá trình độ cơ bản A2",
                        Duration = 45,
                        TotalQuestions = 25,
                        Level = "A2",
                        CreatedDate = DateTime.Now.AddDays(-5),
                        Category = "Trình độ",
                        ImageUrl = "/images/tests/test-a2.jpg",
                        Questions = new List<TestQuestionModel>
                        {
                            new TestQuestionModel
                            {
                                QuestionID = 1,
                                QuestionText = "She ___ a doctor.",
                                Options = new List<string> { "am", "is", "are", "be" },
                                CorrectAnswer = 1
                            },
                            new TestQuestionModel
                            {
                                QuestionID = 2,
                                QuestionText = "Where ___ you from?",
                                Options = new List<string> { "am", "is", "are", "be" },
                                CorrectAnswer = 2
                            }
                        }
                    }
                });
            }
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
                    },
                    new GrammarModel
                    {
                        ID_NP = 4,
                        Title_NP = "Future Simple Tense",
                        Description_NP = "Học cách diễn đạt hành động sẽ xảy ra trong tương lai bằng thì tương lai đơn",
                        Content_NP = "<h2>Thì tương lai đơn (Future Simple Tense)</h2><p>Thì tương lai đơn được dùng để diễn tả:</p><ul><li>Một hành động, sự việc sẽ xảy ra trong tương lai</li><li>Một quyết định tức thời được đưa ra tại thời điểm nói</li><li>Một dự đoán về tương lai</li><li>Một lời hứa, lời đề nghị, lời cảnh báo</li></ul><h3>Cấu trúc:</h3><p><b>Khẳng định:</b></p><ul><li>I/You/He/She/It/We/They + will ('ll) + V (nguyên thể)</li></ul><p><b>Phủ định:</b></p><ul><li>I/You/He/She/It/We/They + will not (won't) + V (nguyên thể)</li></ul><p><b>Nghi vấn:</b></p><ul><li>Will + I/you/he/she/it/we/they + V (nguyên thể)?</li></ul><h3>Dấu hiệu nhận biết:</h3><p>Thường đi với các trạng từ chỉ thời gian tương lai như: tomorrow, next week/month/year, in the future, soon...</p>",
                        TimeUpload_NP = DateTime.Now.AddDays(-18),
                        ID_CD = 2,
                        TopicName = "Basic Grammar",
                        Level = "A2",
                        Examples = new List<string> { "I will call you tomorrow.", "She won't be at home next weekend.", "Will they attend the meeting?" },
                        ProgressPercentage = 30,
                        IsFavorite = false
                    },
                    new GrammarModel
                    {
                        ID_NP = 5,
                        Title_NP = "Present Perfect Tense",
                        Description_NP = "Học cách sử dụng thì hiện tại hoàn thành để nói về hành động đã hoàn thành nhưng có liên quan đến hiện tại",
                        Content_NP = "<h2>Thì hiện tại hoàn thành (Present Perfect Tense)</h2><p>Thì hiện tại hoàn thành được dùng để diễn tả:</p><ul><li>Một hành động đã xảy ra trong quá khứ nhưng có kết quả hoặc ảnh hưởng đến hiện tại</li><li>Một hành động bắt đầu trong quá khứ và vẫn đang tiếp tục ở hiện tại</li><li>Một hành động vừa mới hoàn thành</li><li>Một kinh nghiệm cho tới thời điểm hiện tại</li></ul><h3>Cấu trúc:</h3><p><b>Khẳng định:</b></p><ul><li>I/You/We/They + have ('ve) + V3/ed (quá khứ phân từ)</li><li>He/She/It + has ('s) + V3/ed (quá khứ phân từ)</li></ul><p><b>Phủ định:</b></p><ul><li>I/You/We/They + have not (haven't) + V3/ed</li><li>He/She/It + has not (hasn't) + V3/ed</li></ul><p><b>Nghi vấn:</b></p><ul><li>Have + I/you/we/they + V3/ed?</li><li>Has + he/she/it + V3/ed?</li></ul><h3>Dấu hiệu nhận biết:</h3><p>Thường đi với: since, for, just, already, yet, ever, never, recently, so far, up to now...</p>",
                        TimeUpload_NP = DateTime.Now.AddDays(-15),
                        ID_CD = 3,
                        TopicName = "Intermediate Grammar",
                        Level = "B1",
                        Examples = new List<string> { "I have lived in this city for 5 years.", "She hasn't finished her homework yet.", "Have you ever visited Paris?" },
                        ProgressPercentage = 60,
                        IsFavorite = false
                    },
                    new GrammarModel
                    {
                        ID_NP = 6,
                        Title_NP = "Past Perfect Tense",
                        Description_NP = "Học cách sử dụng thì quá khứ hoàn thành để nói về hành động xảy ra trước một hành động khác trong quá khứ",
                        Content_NP = "<h2>Thì quá khứ hoàn thành (Past Perfect Tense)</h2><p>Thì quá khứ hoàn thành được dùng để diễn tả:</p><ul><li>Một hành động xảy ra trước một hành động khác trong quá khứ</li><li>Một hành động đã hoàn thành trước một mốc thời gian trong quá khứ</li><li>Một hành động đã xảy ra trong quá khứ nhưng kết quả của nó ảnh hưởng đến một hành động khác trong quá khứ</li></ul><h3>Cấu trúc:</h3><p><b>Khẳng định:</b></p><ul><li>I/You/He/She/It/We/They + had ('d) + V3/ed (quá khứ phân từ)</li></ul><p><b>Phủ định:</b></p><ul><li>I/You/He/She/It/We/They + had not (hadn't) + V3/ed</li></ul><p><b>Nghi vấn:</b></p><ul><li>Had + I/you/he/she/it/we/they + V3/ed?</li></ul><h3>Dấu hiệu nhận biết:</h3><p>Thường đi với: before, after, by the time, when, until, already, just...</p><p>Thường đi kèm với thì quá khứ đơn trong câu có hai mệnh đề: hành động xảy ra trước dùng thì quá khứ hoàn thành, hành động xảy ra sau dùng thì quá khứ đơn.</p>",
                        TimeUpload_NP = DateTime.Now.AddDays(-12),
                        ID_CD = 3,
                        TopicName = "Intermediate Grammar",
                        Level = "B1",
                        Examples = new List<string> { "I had already left when she called.", "She hadn't finished her work before the deadline.", "Had they arrived by the time you left?" },
                        ProgressPercentage = 50,
                        IsFavorite = true
                    },
                    new GrammarModel
                    {
                        ID_NP = 7,
                        Title_NP = "Reported Speech",
                        Description_NP = "Học cách chuyển đổi câu trực tiếp thành câu gián tiếp trong tiếng Anh",
                        Content_NP = "<h2>Câu tường thuật (Reported Speech)</h2><p>Câu tường thuật là cách chuyển đổi lời nói trực tiếp thành lời nói gián tiếp, được sử dụng khi thuật lại những gì người khác đã nói.</p><h3>Các quy tắc chuyển đổi:</h3><p><b>1. Đại từ:</b></p><ul><li>Đổi ngôi thứ nhất (I, me, my, mine, we, us, our) theo ngôi của người thuật lại</li><li>Đổi ngôi thứ hai (you, your, yours) theo đối tượng của câu trực tiếp</li><li>Ngôi thứ ba (he, she, they, his, her, their) thường giữ nguyên</li></ul><p><b>2. Thì của động từ:</b></p><ul><li>Present Simple → Past Simple</li><li>Present Continuous → Past Continuous</li><li>Present Perfect → Past Perfect</li><li>Past Simple → Past Perfect</li><li>Will → Would</li><li>Can → Could</li><li>May → Might</li><li>Must → Had to</li></ul><p><b>3. Trạng từ chỉ thời gian và nơi chốn:</b></p><ul><li>Now → Then</li><li>Today → That day</li><li>Yesterday → The day before/The previous day</li><li>Tomorrow → The next day/The following day</li><li>Here → There</li><li>This → That</li><li>These → Those</li></ul><h3>Câu trực tiếp và gián tiếp:</h3><p><b>Câu trần thuật:</b></p><ul><li>Trực tiếp: \"I am studying English.\" (She said)</li><li>Gián tiếp: She said (that) she was studying English.</li></ul><p><b>Câu hỏi:</b></p><ul><li>Trực tiếp: \"Do you speak English?\" (He asked me)</li><li>Gián tiếp: He asked me if/whether I spoke English.</li></ul><p><b>Câu mệnh lệnh:</b></p><ul><li>Trực tiếp: \"Close the door!\" (She told me)</li><li>Gián tiếp: She told me to close the door.</li></ul>",
                        TimeUpload_NP = DateTime.Now.AddDays(-10),
                        ID_CD = 4,
                        TopicName = "Intermediate Grammar",
                        Level = "B2",
                        Examples = new List<string> { "Direct: \"I will call you tomorrow.\" → Reported: She said she would call me the next day.", "Direct: \"Have you finished your homework?\" → Reported: She asked me if I had finished my homework.", "Direct: \"Don't be late!\" → Reported: He told me not to be late." },
                        ProgressPercentage = 40,
                        IsFavorite = false
                    },
                    new GrammarModel
                    {
                        ID_NP = 8,
                        Title_NP = "Conditional Sentences",
                        Description_NP = "Học cách sử dụng các loại câu điều kiện khác nhau trong tiếng Anh",
                        Content_NP = "<h2>Câu điều kiện (Conditional Sentences)</h2><p>Câu điều kiện được dùng để diễn tả một hành động hoặc sự việc chỉ xảy ra khi một điều kiện nào đó được thỏa mãn.</p><h3>Các loại câu điều kiện:</h3><p><b>1. Câu điều kiện loại 0 (Zero Conditional):</b></p><ul><li><b>Cấu trúc:</b> If + S + V(hiện tại đơn), S + V(hiện tại đơn)</li><li><b>Ý nghĩa:</b> Diễn tả một sự thật, quy luật tự nhiên hoặc thói quen</li><li><b>Ví dụ:</b> If you heat water to 100°C, it boils.</li></ul><p><b>2. Câu điều kiện loại 1 (First Conditional):</b></p><ul><li><b>Cấu trúc:</b> If + S + V(hiện tại đơn), S + will/can/may + V(nguyên thể)</li><li><b>Ý nghĩa:</b> Diễn tả một điều kiện có thể xảy ra ở hiện tại hoặc tương lai</li><li><b>Ví dụ:</b> If it rains tomorrow, I will stay at home.</li></ul><p><b>3. Câu điều kiện loại 2 (Second Conditional):</b></p><ul><li><b>Cấu trúc:</b> If + S + V(quá khứ đơn), S + would/could/might + V(nguyên thể)</li><li><b>Ý nghĩa:</b> Diễn tả một điều kiện không có thật ở hiện tại hoặc một tình huống khó có thể xảy ra trong tương lai</li><li><b>Ví dụ:</b> If I had a lot of money, I would travel around the world.</li></ul><p><b>4. Câu điều kiện loại 3 (Third Conditional):</b></p><ul><li><b>Cấu trúc:</b> If + S + had + V3/ed, S + would/could/might + have + V3/ed</li><li><b>Ý nghĩa:</b> Diễn tả một điều kiện không có thật trong quá khứ và kết quả giả định của nó</li><li><b>Ví dụ:</b> If I had studied harder, I would have passed the exam.</li></ul><p><b>5. Câu điều kiện hỗn hợp (Mixed Conditional):</b></p><ul><li><b>Cấu trúc:</b> If + S + had + V3/ed, S + would/could/might + V(nguyên thể) (hoặc ngược lại)</li><li><b>Ý nghĩa:</b> Điều kiện xảy ra ở thời điểm này nhưng kết quả lại ở thời điểm khác</li><li><b>Ví dụ:</b> If I had studied medicine (quá khứ), I would be a doctor now (hiện tại).</li></ul>",
                        TimeUpload_NP = DateTime.Now.AddDays(-8),
                        ID_CD = 4,
                        TopicName = "Intermediate Grammar",
                        Level = "B2",
                        Examples = new List<string> { "If you study hard, you will pass the exam.", "If I were you, I would take that job.", "If she had arrived on time, she wouldn't have missed the meeting." },
                        ProgressPercentage = 45,
                        IsFavorite = true
                    },
                    new GrammarModel
                    {
                        ID_NP = 9,
                        Title_NP = "Passive Voice",
                        Description_NP = "Học cách sử dụng câu bị động trong tiếng Anh để nhấn mạnh đối tượng bị tác động",
                        Content_NP = "<h2>Câu bị động (Passive Voice)</h2><p>Câu bị động được sử dụng khi muốn nhấn mạnh đến đối tượng bị tác động hoặc khi không biết/không cần biết chủ thể thực hiện hành động.</p><h3>Cấu trúc câu bị động:</h3><p><b>Công thức chung:</b> Subject + to be (ở thì tương ứng) + past participle (V3/ed) + (by + agent)</p><p><b>Các thì trong câu bị động:</b></p><ul><li><b>Hiện tại đơn:</b> S + am/is/are + V3/ed<br>Active: They build houses. → Passive: Houses are built (by them).</li><li><b>Hiện tại tiếp diễn:</b> S + am/is/are + being + V3/ed<br>Active: They are building a house. → Passive: A house is being built (by them).</li><li><b>Hiện tại hoàn thành:</b> S + have/has + been + V3/ed<br>Active: They have built a house. → Passive: A house has been built (by them).</li><li><b>Quá khứ đơn:</b> S + was/were + V3/ed<br>Active: They built a house. → Passive: A house was built (by them).</li><li><b>Quá khứ tiếp diễn:</b> S + was/were + being + V3/ed<br>Active: They were building a house. → Passive: A house was being built (by them).</li><li><b>Quá khứ hoàn thành:</b> S + had + been + V3/ed<br>Active: They had built a house. → Passive: A house had been built (by them).</li><li><b>Tương lai đơn:</b> S + will + be + V3/ed<br>Active: They will build a house. → Passive: A house will be built (by them).</li><li><b>Tương lai hoàn thành:</b> S + will + have + been + V3/ed<br>Active: They will have built a house. → Passive: A house will have been built (by them).</li></ul><h3>Trường hợp đặc biệt:</h3><p><b>1. Động từ khuyết thiếu:</b></p><ul><li>S + modal + be + V3/ed<br>Active: You must clean the room. → Passive: The room must be cleaned.</li></ul><p><b>2. Câu với hai tân ngữ:</b></p><ul><li>Active: He gave me a book.<br>Passive: I was given a book (by him). HOẶC: A book was given to me (by him).</li></ul><p><b>3. Câu hỏi:</b></p><ul><li>Active: Who wrote this book?<br>Passive: By whom was this book written? HOẶC: Who was this book written by?</li></ul>",
                        TimeUpload_NP = DateTime.Now.AddDays(-6),
                        ID_CD = 5,
                        TopicName = "Advanced Grammar",
                        Level = "C1",
                        Examples = new List<string> { "The novel was written by a famous author.", "The new bridge is being constructed by a local company.", "All the tickets had been sold before we arrived." },
                        ProgressPercentage = 35,
                        IsFavorite = false
                    },
                    new GrammarModel
                    {
                        ID_NP = 10,
                        Title_NP = "Inversion",
                        Description_NP = "Học cách sử dụng đảo ngữ để nhấn mạnh và tạo phong cách trong tiếng Anh",
                        Content_NP = "<h2>Đảo ngữ (Inversion)</h2><p>Đảo ngữ là cấu trúc đặc biệt trong tiếng Anh khi trợ động từ hoặc động từ được đặt trước chủ ngữ, thường để nhấn mạnh hoặc tạo phong cách trong văn viết.</p><h3>Các loại đảo ngữ:</h3><p><b>1. Đảo ngữ hoàn toàn (Full inversion):</b> Động từ chính đứng trước chủ ngữ</p><ul><li><b>Với trạng từ chỉ nơi chốn:</b> In walked the manager. (= The manager walked in.)</li><li><b>Khi kể chuyện:</b> \"I don't like it,\" said John. (= John said, \"I don't like it.\")</li><li><b>Với Here/There:</b> Here comes the bus. (= The bus comes here.)</li></ul><p><b>2. Đảo ngữ một phần (Partial inversion):</b> Trợ động từ đứng trước chủ ngữ</p><ul><li><b>Sau các cụm từ phủ định:</b><br>Never have I seen such a beautiful sunset. (= I have never seen such a beautiful sunset.)</li><li><b>Sau hardly/scarcely...when, no sooner...than:</b><br>Hardly had I arrived when it started to rain. (= I had hardly arrived when it started to rain.)</li><li><b>Sau only + trạng ngữ:</b><br>Only then did I realize my mistake. (= I only realized my mistake then.)</li><li><b>Sau các trạng từ phủ định đầu câu:</b><br>Seldom does he visit his parents. (= He seldom visits his parents.)</li><li><b>Với các biểu hiện: not only...but also, so...that, such...that, neither...nor, nowhere:</b><br>Not only did she pass the exam, but she also got the highest score.</li></ul><p><b>3. Đảo ngữ trong câu điều kiện:</b></p><ul><li><b>Câu điều kiện loại 1:</b> Should it rain, we will cancel the picnic. (= If it rains, we will cancel the picnic.)</li><li><b>Câu điều kiện loại 2:</b> Were I in your position, I would accept the offer. (= If I were in your position, I would accept the offer.)</li><li><b>Câu điều kiện loại 3:</b> Had I known about the problem, I would have fixed it. (= If I had known about the problem, I would have fixed it.)</li></ul><p><b>4. Với as, though và than:</b></p><ul><li>Child as he is, he knows a lot about computers. (= Although he is a child, he knows a lot about computers.)</li><li>Try as I might, I couldn't lift the box. (= Although I tried hard, I couldn't lift the box.)</li></ul><p><b>5. So, neither, nor (để đồng ý với câu trước):</b></p><ul><li>\"I can swim.\" \"So can I.\" (= I can swim too.)</li><li>\"I don't like coffee.\" \"Neither do I.\" (= I don't like coffee either.)</li></ul>",
                        TimeUpload_NP = DateTime.Now.AddDays(-5),
                        ID_CD = 5,
                        TopicName = "Advanced Grammar",
                        Level = "C1",
                        Examples = new List<string> { "Never before had I seen such a magnificent view.", "Hardly had I stepped outside when it began to rain heavily.", "Had I known you were coming, I would have prepared dinner." },
                        ProgressPercentage = 30,
                        IsFavorite = true
                    },
                    new GrammarModel
                    {
                        ID_NP = 11,
                        Title_NP = "Cleft Sentences",
                        Description_NP = "Học cách sử dụng câu chẻ để nhấn mạnh thông tin quan trọng trong câu",
                        Content_NP = "<h2>Câu chẻ (Cleft Sentences)</h2><p>Câu chẻ là một cấu trúc được sử dụng để nhấn mạnh một phần cụ thể của câu bằng cách \"chẻ\" câu đơn thành hai mệnh đề, một với \"it is/was\" hoặc \"what\" và một mệnh đề quan hệ.</p><h3>Các loại câu chẻ:</h3><p><b>1. It-cleft (nhấn mạnh đối tượng, trạng từ, hoặc tân ngữ):</b></p><ul><li><b>Cấu trúc:</b> It + be + từ/cụm từ được nhấn mạnh + that/who/when... + phần còn lại của câu</li><li><b>Câu gốc:</b> Sarah bought a new car yesterday.</li><li><b>Nhấn mạnh chủ ngữ:</b> It was Sarah who/that bought a new car yesterday.</li><li><b>Nhấn mạnh đối tượng:</b> It was a new car that Sarah bought yesterday.</li><li><b>Nhấn mạnh thời gian:</b> It was yesterday that Sarah bought a new car.</li></ul><p><b>2. Wh-cleft (nhấn mạnh một hành động hoặc tình huống cả câu):</b></p><ul><li><b>Cấu trúc:</b> What + phần còn lại của câu + be + từ/cụm từ được nhấn mạnh</li><li><b>Câu gốc:</b> Sarah bought a new car yesterday.</li><li><b>Nhấn mạnh hành động:</b> What Sarah did yesterday was (to) buy a new car.</li><li><b>Nhấn mạnh chủ ngữ:</b> What bought a new car yesterday was Sarah.</li></ul><p><b>3. All-cleft:</b></p><ul><li><b>Cấu trúc:</b> All + that/who... + phần còn lại của câu</li><li><b>Ví dụ:</b> All (that) I want is a good night's sleep.</li></ul><p><b>4. The thing/person/place/reason... cleft:</b></p><ul><li><b>Cấu trúc:</b> The + thing/person/place/reason... + that/who/where/why... + phần còn lại của câu + be + thông tin được nhấn mạnh</li><li><b>Ví dụ:</b> The place where we first met was in Paris.</li><li><b>Ví dụ:</b> The reason (why) I called you was to invite you to dinner.</li></ul><h3>Mục đích sử dụng câu chẻ:</h3><ul><li>Nhấn mạnh một phần cụ thể của câu</li><li>Tạo sự tương phản</li><li>Làm cho thông tin quan trọng nổi bật</li><li>Tạo sự liên kết mạch lạc trong văn bản</li></ul>",
                        TimeUpload_NP = DateTime.Now.AddDays(-3),
                        ID_CD = 6,
                        TopicName = "Advanced Grammar",
                        Level = "C2",
                        Examples = new List<string> { "It was in 2010 that we moved to London.", "What I need most is some time to myself.", "The reason why she left early was that she had another appointment." },
                        ProgressPercentage = 25,
                        IsFavorite = false
                    },
                    new GrammarModel
                    {
                        ID_NP = 12,
                        Title_NP = "Subjunctive Mood",
                        Description_NP = "Tìm hiểu về thức giả định và cách sử dụng trong tiếng Anh trang trọng và học thuật",
                        Content_NP = "<h2>Thức giả định (Subjunctive Mood)</h2><p>Thức giả định là một dạng ngữ pháp được sử dụng để diễn tả mong ước, giả định, khả năng, đề nghị hay đòi hỏi không có thực trong hiện tại hoặc khó có thể thực hiện được. Thức giả định thường xuất hiện trong ngôn ngữ trang trọng, hàn lâm và các biểu thức cố định.</p><h3>Các dạng của thức giả định:</h3><p><b>1. Thức giả định hiện tại (Present Subjunctive):</b></p><ul><li><b>Cấu trúc:</b> Động từ nguyên mẫu không có 'to' cho tất cả các ngôi</li><li><b>Sử dụng:</b></li><li>Sau các động từ/cụm từ chỉ yêu cầu, đòi hỏi, đề nghị: suggest, recommend, insist, demand, request, propose, require, be essential, be crucial...</li><li><b>Ví dụ:</b> I suggest that he <b>go</b> to the doctor. (NOT: goes)</li><li><b>Ví dụ:</b> The teacher recommended that everyone <b>be</b> present for the exam. (NOT: is/are)</li></ul><p><b>2. Thức giả định quá khứ (Past Subjunctive):</b></p><ul><li><b>Cấu trúc:</b> Sử dụng 'were' cho tất cả các ngôi</li><li><b>Sử dụng:</b></li><li>Trong câu điều kiện loại 2</li><li>Sau các cấu trúc: as if, as though, if only, wish, suppose, I'd rather, it's high time...</li><li><b>Ví dụ:</b> If I <b>were</b> you, I would accept the job offer. (NOT: was - mặc dù 'was' cũng được chấp nhận trong ngôn ngữ không trang trọng)</li><li><b>Ví dụ:</b> She acts as if she <b>were</b> the boss. (NOT: was - trong ngôn ngữ trang trọng)</li></ul><p><b>3. Thức giả định hoàn thành (Perfect Subjunctive):</b></p><ul><li><b>Cấu trúc:</b> Should/would/could/might + have + V3/ed</li><li><b>Sử dụng:</b></li><li>Để nói về tình huống không có thật trong quá khứ</li><li><b>Ví dụ:</b> If only he <b>had been</b> more careful. (= I wish he had been more careful, but he wasn't.)</li></ul><p><b>4. Các biểu thức cố định có thức giả định:</b></p><ul><li>God <b>bless</b> you!</li><li>Long <b>live</b> the King!</li><li><b>Be</b> that as it may...</li><li><b>Come</b> what may...</li><li>So <b>be</b> it.</li><li>Heaven <b>forbid</b>!</li><li>Truth <b>be</b> told...</li><li>Far <b>be</b> it from me to criticize, but...</li></ul><p><b>5. Các cấu trúc thường sử dụng thức giả định:</b></p><ul><li><b>I wish (that)...</b> I wish (that) he <b>were</b> here now.</li><li><b>If only...</b> If only it <b>were</b> summer.</li><li><b>As if/as though...</b> He talks as if he <b>knew</b> everything.</li><li><b>Would rather that...</b> I would rather that you <b>didn't leave</b> yet.</li><li><b>It's time that...</b> It's time that we <b>went</b> home.</li><li><b>Lest...</b> He spoke quietly lest the baby <b>wake up</b>.</li></ul><p>Thức giả định là một khía cạnh tinh tế của tiếng Anh, thường xuất hiện trong văn phong trang trọng và học thuật, giúp diễn đạt các ý tưởng phức tạp về điều kiện, giả định và mong ước.</p>",
                        TimeUpload_NP = DateTime.Now.AddDays(-1),
                        ID_CD = 6,
                        TopicName = "Advanced Grammar",
                        Level = "C2",
                        Examples = new List<string> { "The committee insists that each member be present at the meeting.", "If I were you, I would reconsider that decision.", "I wish she were more understanding about the situation." },
                        ProgressPercentage = 20,
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
