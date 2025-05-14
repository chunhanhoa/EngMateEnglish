using System;
using System.Collections.Generic;

namespace TiengAnh.Models
{
    public class TestProgressModel
    {
        public int TotalAvailableTests { get; set; }
        public List<CompletedTestModel> CompletedTests { get; set; }
        public int CompletedCount { get; set; }
        public double CompletionPercentage { get; set; }
        public double AverageScore { get; set; }
        public Dictionary<string, CategoryProgressModel> CategoryProgress { get; set; }
        public Dictionary<string, LevelProgressModel> LevelProgress { get; set; }
        public List<CompletedTestModel> RecentCompletions { get; set; }
    }

    public class CategoryProgressModel
    {
        public int TotalTests { get; set; }
        public int CompletedTests { get; set; }
        public double CompletionPercentage { get; set; }
        public double AverageScore { get; set; }
    }

    public class LevelProgressModel
    {
        public int TotalTests { get; set; }
        public int CompletedTests { get; set; }
        public double CompletionPercentage { get; set; }
        public double AverageScore { get; set; }
    }
}
