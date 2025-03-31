using System;
using System.Collections.Generic;

namespace TiengAnh.Models
{
    public class ProgressStatsModel
    {
        public int TotalVocabulary { get; set; }
        public int LearnedVocabulary { get; set; }
        public int VocabularyPercentage { get; set; }
        
        public int TotalGrammar { get; set; }
        public int LearnedGrammar { get; set; }
        public int GrammarPercentage { get; set; }
        
        public int TotalExercises { get; set; }
        public int CompletedExercises { get; set; }
        public int ExercisesPercentage { get; set; }
        
        public int TotalPoints { get; set; }
        public string Level { get; set; } = string.Empty;
        public List<LastCompletedItemModel> RecentActivities { get; set; } = new List<LastCompletedItemModel>();
        public List<CompletedTopicModel> CompletedTopics { get; set; } = new List<CompletedTopicModel>();
    }
}
