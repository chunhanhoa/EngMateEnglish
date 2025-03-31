using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;  // Add this namespace for Activity class
using System.Security.Claims;
using TiengAnh.Data;
using TiengAnh.Models;

namespace TiengAnh.Controllers
{
    public class ProgressController : Controller
    {
        private readonly ILogger<ProgressController> _logger;
        private readonly HocTiengAnhContext _context;

        public ProgressController(ILogger<ProgressController> logger, HocTiengAnhContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy ID người dùng từ claims
            var userIdStr = User.FindFirstValue("UserID");
            
            if (!int.TryParse(userIdStr, out int userId))
            {
                return NotFound();
            }

            // Lấy thông tin người dùng từ database
            var dbUser = await _context.TaiKhoans
                .FirstOrDefaultAsync(u => u.IdTk == userId);

            if (dbUser == null)
            {
                return NotFound();
            }

            // Đếm số từ vựng đã học
            var vocabularyCount = await _context.TuVungs.CountAsync();
            var vocabularyLearned = await _context.TienTrinhHocs
                .CountAsync(t => t.IdTk == userId && t.TypeTth == "TuVung");

            // Đếm số ngữ pháp đã học
            var grammarCount = await _context.NguPhaps.CountAsync();
            var grammarLearned = await _context.TienTrinhHocs
                .CountAsync(t => t.IdTk == userId && t.TypeTth == "NguPhap");

            // Đếm số bài tập đã làm
            var exerciseCount = await _context.BaiTaps.CountAsync();
            var exercisesCompleted = await _context.TienTrinhHocs
                .CountAsync(t => t.IdTk == userId && t.TypeTth == "BaiTap");
                
            // Tính phần trăm hoàn thành
            var vocabularyPercentage = vocabularyCount > 0 ? (vocabularyLearned * 100) / vocabularyCount : 0;
            var grammarPercentage = grammarCount > 0 ? (grammarLearned * 100) / grammarCount : 0;
            var exercisePercentage = exerciseCount > 0 ? (exercisesCompleted * 100) / exerciseCount : 0;
            
            // Tạo model thống kê
            var progressModel = new ProgressStatsModel
            {
                TotalVocabulary = vocabularyCount,
                LearnedVocabulary = vocabularyLearned,
                VocabularyPercentage = vocabularyPercentage,
                
                TotalGrammar = grammarCount,
                LearnedGrammar = grammarLearned,
                GrammarPercentage = grammarPercentage,
                
                TotalExercises = exerciseCount,
                CompletedExercises = exercisesCompleted,
                ExercisesPercentage = exercisePercentage,
            };
            
            // Lấy các hoạt động gần đây
            var recentActivities = await _context.TienTrinhHocs
                .Where(t => t.IdTk == userId)
                .OrderByDescending(t => t.LastTimeStudyTth)
                .Take(10)
                .Select(t => new LastCompletedItemModel
                {
                    Id = t.IdTypeTth,
                    Type = t.TypeTth ?? string.Empty,
                    Title = t.TypeTth ?? string.Empty, // Sẽ được cập nhật bởi code phía dưới
                    CompletedDate = t.LastTimeStudyTth,
                    Score = 0 // Mặc định
                })
                .ToListAsync();
                
            // Cập nhật thông tin chi tiết cho các hoạt động
            foreach (var activity in recentActivities)
            {
                if (activity.Type == "TuVung")
                {
                    var vocab = await _context.TuVungs
                        .FirstOrDefaultAsync(v => v.IdTv == activity.Id);
                    
                    if (vocab != null)
                    {
                        activity.Title = vocab.WordTv ?? string.Empty;
                    }
                }
                else if (activity.Type == "NguPhap")
                {
                    var grammar = await _context.NguPhaps
                        .FirstOrDefaultAsync(g => g.IdNp == activity.Id);
                    
                    if (grammar != null)
                    {
                        activity.Title = grammar.TitleNp ?? string.Empty;
                    }
                }
            }
            
            progressModel.RecentActivities = recentActivities;
            
            // Lấy các chủ đề đã hoàn thành
            var completedTopics = await GetCompletedTopicsAsync(userId);
            progressModel.CompletedTopics = completedTopics;
            
            return View(progressModel);
        }

        public async Task<IActionResult> Favorites()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy ID người dùng từ claims
            var userIdStr = User.FindFirstValue("UserID");
            
            if (!int.TryParse(userIdStr, out int userId))
            {
                return NotFound();
            }

            try {
                // Phương pháp 1: Truy vấn SQL trực tiếp - tránh sử dụng Entity Framework
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = @"
                        SELECT YT.*, TV.word_TV, TV.meaning_TV, TV.image_TV, TV.level_TV, NP.title_NP, NP.discription_NP
                        FROM YeuThich YT
                        LEFT JOIN TuVung TV ON YT.ID_type_YT = TV.ID_TV AND YT.type_YT = 'TuVung'
                        LEFT JOIN NguPhap NP ON YT.ID_type_YT = NP.ID_NP AND YT.type_YT = 'NguPhap'
                        WHERE YT.ID_TK = @userId
                        ORDER BY YT.date_check_YT DESC";

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@userId";
                    parameter.Value = userId;
                    command.Parameters.Add(parameter);

                    _context.Database.OpenConnection();
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var favoriteViewModels = new List<YeuThichViewModel>();
                        var vocabularies = new List<VocabularyModel>();
                        var grammars = new List<GrammarModel>();
                        
                        while (await reader.ReadAsync())
                        {
                            var favoriteType = reader.GetString(reader.GetOrdinal("type_YT"));
                            var favoriteId = reader.GetInt32(reader.GetOrdinal("ID_YT"));
                            var dateAdded = reader.IsDBNull(reader.GetOrdinal("date_check_YT")) 
                                ? DateTime.Now 
                                : reader.GetDateTime(reader.GetOrdinal("date_check_YT"));
                            var itemId = reader.GetInt32(reader.GetOrdinal("ID_type_YT"));
                            
                            var viewModel = new YeuThichViewModel
                            {
                                ID = favoriteId,
                                Type = favoriteType,
                                ItemID = itemId,
                                DateAdded = dateAdded
                            };
                            
                            if (favoriteType == "TuVung" && !reader.IsDBNull(reader.GetOrdinal("word_TV")))
                            {
                                viewModel.Title = reader.GetString(reader.GetOrdinal("word_TV"));
                                viewModel.Description = reader.IsDBNull(reader.GetOrdinal("meaning_TV")) ? "" : reader.GetString(reader.GetOrdinal("meaning_TV"));
                                viewModel.ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_TV")) ? "/images/vocabulary/default.jpg" : reader.GetString(reader.GetOrdinal("image_TV"));
                                
                                var vocab = new VocabularyModel
                                {
                                    ID_TV = itemId,
                                    Word_TV = viewModel.Title,
                                    Meaning_TV = viewModel.Description,
                                    Image_TV = viewModel.ImageUrl,
                                    Level_TV = reader.IsDBNull(reader.GetOrdinal("level_TV")) ? "A1" : reader.GetString(reader.GetOrdinal("level_TV"))
                                };
                                vocabularies.Add(vocab);
                            }
                            else if (favoriteType == "NguPhap" && !reader.IsDBNull(reader.GetOrdinal("title_NP")))
                            {
                                viewModel.Title = reader.GetString(reader.GetOrdinal("title_NP"));
                                viewModel.Description = reader.IsDBNull(reader.GetOrdinal("discription_NP")) ? "" : reader.GetString(reader.GetOrdinal("discription_NP"));
                                viewModel.ImageUrl = "/images/grammar-icon.png";
                                
                                var grammar = new GrammarModel
                                {
                                    ID_NP = itemId,
                                    Title_NP = viewModel.Title,
                                    Description_NP = viewModel.Description
                                };
                                grammars.Add(grammar);
                            }
                            
                            favoriteViewModels.Add(viewModel);
                        }
                        
                        ViewBag.VocabularyItems = vocabularies;
                        ViewBag.GrammarItems = grammars;
                        
                        return View(favoriteViewModels);
                    }
                }
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = "Có lỗi khi tải dữ liệu mục yêu thích: " + ex.Message
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFavorite(int id, string type)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này." });
            }

            // Lấy ID người dùng từ claims
            var userIdStr = User.FindFirstValue("UserID");
            
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Json(new { success = false, message = "Không xác định được người dùng." });
            }

            try
            {
                // Tìm mục yêu thích trong database
                var favorite = await _context.YeuThiches
                    .FirstOrDefaultAsync(y => y.IdTk == userId && y.TypeYt == type && y.IdYtType == id); // Sử dụng thuộc tính IdYtType
                
                if (favorite != null)
                {
                    _context.YeuThiches.Remove(favorite);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Đã xóa khỏi mục yêu thích." });
                }
                
                return Json(new { success = false, message = "Không tìm thấy mục yêu thích." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa mục yêu thích {Type} {Id}", type, id);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa mục yêu thích." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite([FromBody] ToggleFavoriteRequest request)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (!(User.Identity?.IsAuthenticated ?? false))  // Fix null reference warning by inverting the condition
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để yêu thích." });
            }

            // Lấy ID người dùng từ claims
            var userIdStr = User.FindFirstValue("UserID");
            
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Json(new { success = false, message = "Không xác định được người dùng." });
            }

            try
            {
                // Kiểm tra xem mục đã được yêu thích chưa
                var existingFavorite = await _context.YeuThiches
                    .FirstOrDefaultAsync(y => y.IdTk == userId && y.TypeYt == request.ItemType && y.IdYtType == request.ItemId); // Sử dụng thuộc tính IdYtType
                
                bool isFavorite;
                
                if (existingFavorite != null)
                {
                    // Nếu đã yêu thích rồi thì xóa (bỏ yêu thích)
                    _context.YeuThiches.Remove(existingFavorite);
                    isFavorite = false;
                }
                else
                {
                    // Nếu chưa yêu thích thì thêm mới (yêu thích)
                    var newFavorite = new YeuThich
                    {
                        IdTk = userId,
                        TypeYt = request.ItemType,
                        IdYtType = request.ItemId, // Sử dụng thuộc tính IdYtType
                        DateCheckYt = DateTime.Now
                    };
                    
                    _context.YeuThiches.Add(newFavorite);
                    isFavorite = true;
                }
                
                await _context.SaveChangesAsync();
                
                return Json(new FavoriteResponse
                {
                    Success = true,
                    Message = isFavorite ? "Đã thêm vào mục yêu thích." : "Đã xóa khỏi mục yêu thích.",
                    IsFavorite = isFavorite
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi toggle yêu thích {Type} {Id}", request.ItemType, request.ItemId);
                return Json(new FavoriteResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi cập nhật mục yêu thích.",
                    IsFavorite = false
                });
            }
        }

        private async Task<List<CompletedTopicModel>> GetCompletedTopicsAsync(int userId)
        {
            var result = new List<CompletedTopicModel>();
            
            // Lấy danh sách tất cả chủ đề
            var topics = await _context.ChuDes.ToListAsync();
            
            foreach (var topic in topics)
            {
                // Đếm số từ vựng trong chủ đề
                var vocabCount = await _context.TuVungs
                    .CountAsync(v => v.IdCd == topic.IdCd);
                
                // Đếm số từ vựng đã học trong chủ đề
                var learnedCount = await _context.TienTrinhHocs
                    .Where(t => t.IdTk == userId && t.TypeTth == "TuVung")
                    .Join(_context.TuVungs,
                        history => history.IdTypeTth,
                        vocab => vocab.IdTv,
                        (history, vocab) => new { vocab.IdCd })
                    .CountAsync(joined => joined.IdCd == topic.IdCd);
                
                // Tính phần trăm hoàn thành
                int completionPercentage = vocabCount > 0 ? (learnedCount * 100) / vocabCount : 0;
                
                result.Add(new CompletedTopicModel
                {
                    TopicId = topic.IdCd,
                    TopicName = topic.NameCd,
                    CompletionPercentage = completionPercentage
                });
            }
            
            // Sắp xếp theo phần trăm hoàn thành giảm dần
            return result.OrderByDescending(t => t.CompletionPercentage).ToList();
        }

        private string GetLevelFromTitle(string title)
        {
            // Thử xác định cấp độ từ tiêu đề
            title = title.ToLower();
            
            if (title.Contains("elementary") || title.Contains("beginner"))
                return "A1";
            if (title.Contains("pre-intermediate"))
                return "A2";
            if (title.Contains("intermediate"))
                return "B1";
            if (title.Contains("upper-intermediate"))
                return "B2";
            if (title.Contains("advanced"))
                return "C1";
            if (title.Contains("proficient") || title.Contains("mastery"))
                return "C2";
                
            // Mặc định là B1
            return "B1";
        }

        [HttpGet]
        public IActionResult CheckYeuThichSchema()
        {
            try {
                // Thực hiện truy vấn trực tiếp để lấy thông tin schema
                var columns = _context.Database.ExecuteSqlRaw("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'YeuThich'");
                
                // Thử một truy vấn đơn giản để xem danh sách yêu thích
                var favorites = _context.YeuThiches.Take(1).ToList();
                
                return Json(new { success = true, message = "Truy vấn thành công", data = favorites });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = ex.Message, innerException = ex.InnerException?.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestFavorites()
        {
            try {
                if (!User.Identity?.IsAuthenticated ?? false)
                {
                    return Json(new { error = "Chưa đăng nhập" });
                }

                var userIdStr = User.FindFirstValue("UserID");
                if (!int.TryParse(userIdStr, out int userId))
                {
                    return Json(new { error = "Không xác định được ID người dùng" });
                }
                
                // Truy vấn SQL thuần túy để kiểm tra
                var connection = _context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                
                using var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM YeuThich WHERE ID_TK = {userId}";
                
                using var reader = await command.ExecuteReaderAsync();
                var favorites = new List<object>();
                
                while (await reader.ReadAsync())
                {
                    var favorite = new
                    {
                        ID_YT = reader.GetInt32(0),
                        ID_TK = reader.GetInt32(1),
                        ID_type_YT = reader.GetInt32(2),
                        type_YT = reader.IsDBNull(3) ? null : reader.GetString(3),
                        date_check_YT = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4)
                    };
                    favorites.Add(favorite);
                }
                
                return Json(new { success = true, data = favorites });
            }
            catch (Exception ex) {
                return Json(new { error = ex.Message, stack = ex.StackTrace });
            }
        }
    }
}
