using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TiengAnh.Models;
using TiengAnh.Repositories;

namespace TiengAnh.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VocabularyAdminController : Controller
    {
        private readonly VocabularyRepository _vocabularyRepository;
        private readonly TopicRepository _topicRepository;

        public VocabularyAdminController(
            VocabularyRepository vocabularyRepository,
            TopicRepository topicRepository)
        {
            _vocabularyRepository = vocabularyRepository;
            _topicRepository = topicRepository;
        }

        public async Task<IActionResult> Index()
        {
            var topics = await _topicRepository.GetAllTopicsAsync();
            
            // Update the word count for each topic
            var allVocabularies = await _vocabularyRepository.GetAllAsync();
            foreach (var topic in topics)
            {
                int count = allVocabularies.Count(v => v.ID_CD == topic.ID_CD);
                topic.WordCount = count;
                topic.TotalItems = count;
                topic.TotalWords = count;
            }
            
            return View(topics);
        }

        public async Task<IActionResult> ManageTopic(int id)
        {
            var topic = await _topicRepository.GetByTopicIdAsync(id);
            if (topic == null)
            {
                return NotFound();
            }
            
            var vocabularies = await _vocabularyRepository.GetVocabulariesByTopicIdAsync(id);
            ViewBag.Topic = topic;
            return View(vocabularies);
        }

        public async Task<IActionResult> Create(int topicId)
        {
            var topic = await _topicRepository.GetByTopicIdAsync(topicId);
            if (topic == null)
            {
                return NotFound();
            }
            
            var vocabulary = new VocabularyModel
            {
                ID_CD = topicId,
                TopicName = topic.Name_CD,
                Level_TV = "A1", // Default value
                PartOfSpeech = "noun", // Default value
                ID_LT = "n" // Default value
            };
            
            ViewBag.Topic = topic;
            return View(vocabulary);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VocabularyModel vocabulary)
        {
            if (ModelState.IsValid)
            {
                // Generate a new ID (highest ID + 1)
                var allVocabularies = await _vocabularyRepository.GetAllAsync();
                vocabulary.ID_TV = allVocabularies.Count > 0 ? allVocabularies.Max(v => v.ID_TV) + 1 : 1;
                
                // Initialize other properties
                vocabulary.FavoriteByUsers = new List<string>();
                
                // Add default paths for audio and images if not provided
                if (string.IsNullOrEmpty(vocabulary.Audio_TV))
                {
                    vocabulary.Audio_TV = $"/audio/{vocabulary.Word_TV.ToLower()}.mp3";
                }
                
                if (string.IsNullOrEmpty(vocabulary.Image_TV))
                {
                    vocabulary.Image_TV = $"/images/vocabulary/{vocabulary.TopicName.ToLower()}/{vocabulary.Word_TV.ToLower()}.jpg";
                }
                
                await _vocabularyRepository.CreateAsync(vocabulary);
                
                // Update word count in topic
                var topic = await _topicRepository.GetByTopicIdAsync(vocabulary.ID_CD);
                if (topic != null)
                {
                    topic.WordCount++;
                    topic.TotalItems = topic.WordCount;
                    await _topicRepository.UpdateAsync(topic.ID_CD.ToString(), topic);
                }
                
                TempData["SuccessMessage"] = $"Từ vựng '{vocabulary.Word_TV}' đã được thêm thành công.";
                return RedirectToAction("ManageTopic", new { id = vocabulary.ID_CD });
            }
            
            var topicModel = await _topicRepository.GetByTopicIdAsync(vocabulary.ID_CD);
            ViewBag.Topic = topicModel;
            return View(vocabulary);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var vocabulary = await _vocabularyRepository.GetByVocabularyIdAsync(id);
            if (vocabulary == null)
            {
                return NotFound();
            }
            
            var topic = await _topicRepository.GetByTopicIdAsync(vocabulary.ID_CD);
            ViewBag.Topic = topic;
            
            return View(vocabulary);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VocabularyModel vocabulary)
        {
            if (ModelState.IsValid)
            {
                // Get the existing vocabulary to preserve some properties
                var existingVocabulary = await _vocabularyRepository.GetByVocabularyIdAsync(vocabulary.ID_TV);
                if (existingVocabulary == null)
                {
                    return NotFound();
                }
                
                // Preserve MongoDB Id and other important properties
                vocabulary.Id = existingVocabulary.Id;
                vocabulary.FavoriteByUsers = existingVocabulary.FavoriteByUsers;
                
                // Update using MongoDB document Id
                await _vocabularyRepository.UpdateAsync(vocabulary.Id, vocabulary);
                
                TempData["SuccessMessage"] = $"Từ vựng '{vocabulary.Word_TV}' đã được cập nhật thành công.";
                return RedirectToAction("ManageTopic", new { id = vocabulary.ID_CD });
            }
            
            var topic = await _topicRepository.GetByTopicIdAsync(vocabulary.ID_CD);
            ViewBag.Topic = topic;
            
            return View(vocabulary);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var vocabulary = await _vocabularyRepository.GetByVocabularyIdAsync(id);
            if (vocabulary == null)
            {
                return NotFound();
            }
            
            var topic = await _topicRepository.GetByTopicIdAsync(vocabulary.ID_CD);
            ViewBag.Topic = topic;
            
            return View(vocabulary);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vocabulary = await _vocabularyRepository.GetByVocabularyIdAsync(id);
            if (vocabulary == null)
            {
                return NotFound();
            }
            
            int topicId = vocabulary.ID_CD;
            
            // Use the MongoDB Id instead of the numerical ID
            await _vocabularyRepository.DeleteAsync(vocabulary.Id);
            
            // Update word count in topic
            var topic = await _topicRepository.GetByTopicIdAsync(topicId);
            if (topic != null && topic.WordCount > 0)
            {
                topic.WordCount--;
                topic.TotalItems = topic.WordCount;
                await _topicRepository.UpdateAsync(topic.ID_CD.ToString(), topic);
            }
            
            TempData["SuccessMessage"] = $"Từ vựng '{vocabulary.Word_TV}' đã được xóa thành công.";
            return RedirectToAction("ManageTopic", new { id = topicId });
        }
    }
}
