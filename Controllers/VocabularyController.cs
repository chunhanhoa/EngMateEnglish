using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiengAnh.Data;
using TiengAnh.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace TiengAnh.Controllers
{
    public class VocabularyController : Controller
    {
        private readonly ILogger<VocabularyController> _logger;
        private readonly HocTiengAnhContext _context;

        public VocabularyController(ILogger<VocabularyController> logger, HocTiengAnhContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var topics = await _context.ChuDes.ToListAsync();
            
            var topicModels = topics.Select(topic => new VocabularyTopicModel
            {
                ID_CD = topic.IdCd,
                Name_CD = topic.NameCd ?? string.Empty,
                Description_CD = topic.DiscriptionCd ?? string.Empty,
                IconPath = "/images/icons/topic-default.png", // Default icon path
                BackgroundColor = "#e6f6ff", // Default background color
                WordCount = _context.TuVungs.Count(tv => tv.IdCd == topic.IdCd)
            }).ToList();
            
            return View(topicModels);
        }

        public async Task<IActionResult> Topic(int id)
        {
            var tuVungs = await _context.TuVungs
                .Include(t => t.IdLtNavigation)
                .Where(v => v.IdCd == id)
                .ToListAsync();
                
            var topic = await _context.ChuDes.FindAsync(id);
            
            if (topic == null)
            {
                return NotFound();
            }

            // Chuyển đổi từ TuVung sang VocabularyModel
            var vocabularies = tuVungs.Select(tv => new VocabularyModel
            {
                ID_TV = tv.IdTv,
                Word_TV = tv.WordTv ?? string.Empty,
                Meaning_TV = tv.MeaningTv ?? string.Empty,
                ID_LT = tv.IdLt ?? string.Empty,
                WordType = tv.IdLtNavigation?.NameLt ?? string.Empty,
                PartOfSpeech = tv.IdLtNavigation?.NameLt ?? string.Empty, // Sử dụng LoaiTu.NameLt làm PartOfSpeech
                Example_TV = tv.ExampleTv ?? string.Empty,
                Image_TV = tv.ImageTv ?? string.Empty,
                Audio_TV = tv.AudioTv ?? string.Empty,
                Level_TV = tv.LevelTv ?? string.Empty,
                ID_CD = tv.IdCd,
                TopicName = topic.NameCd ?? string.Empty
            }).ToList();

            // Convert topic to TopicModel
            var topicModel = new TopicModel
            {
                ID_CD = topic.IdCd,
                Name_CD = topic.NameCd ?? string.Empty,
                Description_CD = topic.DiscriptionCd ?? string.Empty,
                IconPath = "/images/icons/topic-default.png",
                BackgroundColor = "#e6f6ff",
                WordCount = vocabularies.Count
            };

            ViewBag.Topic = topicModel;
            return View(vocabularies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var vocabulary = await _context.TuVungs
                .Include(t => t.IdLtNavigation)
                .Include(t => t.IdCdNavigation)
                .FirstOrDefaultAsync(v => v.IdTv == id);
            
            if (vocabulary == null)
            {
                return NotFound();
            }
            
            // Convert TuVung to VocabularyModel
            var vocabularyModel = ConvertToVocabularyModel(vocabulary);

            // Safely handle the favorite check
            bool isFavorite = false;
            
            try
            {
                // Only check if user is authenticated
                if (User.Identity?.IsAuthenticated ?? false)
                {
                    var userIdStr = User.FindFirstValue("UserID");
                    
                    if (int.TryParse(userIdStr, out int userId))
                    {
                        isFavorite = await _context.YeuThiches
                            .AnyAsync(y => y.IdTk == userId && y.TypeYt == "TuVung" && y.IdYtType == id);
                    }
                }
                
                // Explicitly set as a boolean value
                ViewBag.IsFavorite = isFavorite;
            }
            catch (Exception ex)
            {
                // Log the error but continue with the page
                _logger.LogError(ex, "Error checking favorite status for vocabulary {Id}", id);
                ViewBag.IsFavorite = false;
            }

            return View(vocabularyModel);
        }

        public async Task<IActionResult> Flashcard(int id)
        {
            var tuVungs = await _context.TuVungs
                .Include(t => t.IdLtNavigation)
                .Include(t => t.IdCdNavigation)
                .Where(v => v.IdCd == id)
                .ToListAsync();
                
            var topic = await _context.ChuDes.FindAsync(id);
            
            if (topic == null)
            {
                return NotFound();
            }

            // Convert list of TuVung to list of VocabularyModel
            var vocabularyModels = tuVungs.Select(tv => ConvertToVocabularyModel(tv)).ToList();

            // Convert topic to TopicModel
            var topicModel = new TopicModel
            {
                ID_CD = topic.IdCd,
                Name_CD = topic.NameCd ?? string.Empty,
                Description_CD = topic.DiscriptionCd ?? string.Empty,
                IconPath = "/images/icons/topic-default.png",
                BackgroundColor = "#e6f6ff",
                WordCount = tuVungs.Count
            };

            ViewBag.Topic = topicModel;
            return View(vocabularyModels);
        }

        public async Task<IActionResult> Exercise(int id)
        {
            var tuVungs = await _context.TuVungs
                .Include(t => t.IdLtNavigation)
                .Include(t => t.IdCdNavigation)
                .Where(v => v.IdCd == id)
                .ToListAsync();
                
            var topic = await _context.ChuDes.FindAsync(id);
            
            if (topic == null)
            {
                return NotFound();
            }

            // Convert list of TuVung to list of VocabularyModel
            var vocabularyModels = tuVungs.Select(tv => ConvertToVocabularyModel(tv)).ToList();

            // Convert topic to TopicModel
            var topicModel = new TopicModel
            {
                ID_CD = topic.IdCd,
                Name_CD = topic.NameCd ?? string.Empty,
                Description_CD = topic.DiscriptionCd ?? string.Empty,
                IconPath = "/images/icons/topic-default.png",
                BackgroundColor = "#e6f6ff",
                WordCount = tuVungs.Count
            };

            ViewBag.Topic = topicModel;
            return View(vocabularyModels);
        }

        // Helper method to convert TuVung to VocabularyModel
        private VocabularyModel ConvertToVocabularyModel(TuVung vocabulary)
        {
            return new VocabularyModel
            {
                ID_TV = vocabulary.IdTv,
                Word_TV = vocabulary.WordTv ?? string.Empty,
                Meaning_TV = vocabulary.MeaningTv ?? string.Empty,
                ID_LT = vocabulary.IdLt ?? string.Empty,
                WordType = vocabulary.IdLtNavigation?.NameLt ?? string.Empty,
                PartOfSpeech = vocabulary.IdLtNavigation?.NameLt ?? string.Empty,
                Example_TV = vocabulary.ExampleTv ?? string.Empty,
                Image_TV = vocabulary.ImageTv ?? string.Empty,
                Audio_TV = vocabulary.AudioTv ?? string.Empty,
                Level_TV = vocabulary.LevelTv ?? string.Empty,
                ID_CD = vocabulary.IdCd,
                TopicName = vocabulary.IdCdNavigation?.NameCd ?? string.Empty
            };
        }
    }
}
