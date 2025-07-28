using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TiengAnh.Models;

namespace TiengAnh.Services
{
    public class UserStatisticsService
    {
        private readonly MongoDbService _mongoDbService;
        private readonly ILogger<UserStatisticsService> _logger;

        public UserStatisticsService(MongoDbService mongoDbService, ILogger<UserStatisticsService> logger)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
        }

        public async Task<DashboardStatisticsViewModel> GetDashboardStatisticsAsync()
        {
            try
            {
                var userCollection = _mongoDbService.GetCollection<UserModel>("Users");
                var activityCollection = _mongoDbService.GetCollection<UserActivityModel>("UserActivities");

                var now = DateTime.Now;
                var today = now.Date;
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                var monthStart = new DateTime(now.Year, now.Month, 1);

                // Tổng số người dùng
                var totalUsers = await userCollection.CountDocumentsAsync(Builders<UserModel>.Filter.Empty);

                // Người dùng mới hôm nay
                var newUsersToday = await userCollection.CountDocumentsAsync(
                    Builders<UserModel>.Filter.Gte(u => u.RegisterDate, today));

                // Người dùng mới tuần này
                var newUsersThisWeek = await userCollection.CountDocumentsAsync(
                    Builders<UserModel>.Filter.Gte(u => u.RegisterDate, weekStart));

                // Người dùng mới tháng này
                var newUsersThisMonth = await userCollection.CountDocumentsAsync(
                    Builders<UserModel>.Filter.Gte(u => u.RegisterDate, monthStart));

                // Người dùng hoạt động hôm nay
                var activeUsersToday = await userCollection.CountDocumentsAsync(
                    Builders<UserModel>.Filter.Gte(u => u.LastLogin, today));

                // Người dùng hoạt động tuần này
                var activeUsersThisWeek = await userCollection.CountDocumentsAsync(
                    Builders<UserModel>.Filter.Gte(u => u.LastLogin, weekStart));

                // Người dùng hoạt động tháng này
                var activeUsersThisMonth = await userCollection.CountDocumentsAsync(
                    Builders<UserModel>.Filter.Gte(u => u.LastLogin, monthStart));

                // Thống kê theo vai trò
                var usersByRole = new Dictionary<string, int>();
                var roleGroups = await userCollection.Aggregate()
                    .Group(u => u.Role ?? "User", g => new { Role = g.Key, Count = g.Count() })
                    .ToListAsync();

                foreach (var group in roleGroups)
                {
                    usersByRole[group.Role] = group.Count;
                }

                // Thống kê theo level
                var usersByLevel = new Dictionary<string, int>();
                var levelGroups = await userCollection.Aggregate()
                    .Group(u => u.Level ?? "A1", g => new { Level = g.Key, Count = g.Count() })
                    .ToListAsync();

                foreach (var group in levelGroups)
                {
                    usersByLevel[group.Level] = group.Count;
                }

                // Người dùng gần đây
                var recentUsers = await GetRecentUsersAsync();

                // Dữ liệu biểu đồ tăng trưởng
                var userGrowthChart = await GetUserGrowthChartDataAsync();

                // Dữ liệu biểu đồ hoạt động
                var activityChart = await GetActivityChartDataAsync();

                return new DashboardStatisticsViewModel
                {
                    TotalUsers = (int)totalUsers,
                    ActiveUsersToday = (int)activeUsersToday,
                    ActiveUsersThisWeek = (int)activeUsersThisWeek,
                    ActiveUsersThisMonth = (int)activeUsersThisMonth,
                    NewUsersToday = (int)newUsersToday,
                    NewUsersThisWeek = (int)newUsersThisWeek,
                    NewUsersThisMonth = (int)newUsersThisMonth,
                    UsersByRole = usersByRole,
                    UsersByLevel = usersByLevel,
                    RecentUsers = recentUsers,
                    UserGrowthChart = userGrowthChart,
                    ActivityChart = activityChart
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting dashboard statistics: {ex.Message}");
                return new DashboardStatisticsViewModel();
            }
        }

        public async Task<List<UserActivitySummary>> GetRecentUsersAsync(int limit = 10)
        {
            try
            {
                var userCollection = _mongoDbService.GetCollection<UserModel>("Users");
                var users = await userCollection
                    .Find(Builders<UserModel>.Filter.Empty)
                    .SortByDescending(u => u.RegisterDate)
                    .Limit(limit)
                    .ToListAsync();

                var result = new List<UserActivitySummary>();
                var now = DateTime.Now;

                foreach (var user in users)
                {
                    var status = "Offline";
                    if (user.LastLogin.HasValue)
                    {
                        var timeSinceLastLogin = now - user.LastLogin.Value;
                        if (timeSinceLastLogin.TotalMinutes <= 30)
                            status = "Online";
                        else if (timeSinceLastLogin.TotalDays <= 1)
                            status = "Recently Active";
                    }

                    if ((now - user.RegisterDate).TotalDays <= 7)
                        status = "New";

                    result.Add(new UserActivitySummary
                    {
                        UserId = user.Id ?? user.UserId,
                        FullName = user.FullName,
                        Email = user.Email,
                        Avatar = user.Avatar,
                        LastLogin = user.LastLogin ?? user.RegisterDate,
                        RegisterDate = user.RegisterDate,
                        Status = status,
                        DaysActive = user.LastLogin.HasValue ? 
                            (int)(user.LastLogin.Value - user.RegisterDate).TotalDays : 0
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting recent users: {ex.Message}");
                return new List<UserActivitySummary>();
            }
        }

        public async Task<List<ChartDataPoint>> GetUserGrowthChartDataAsync(int days = 30)
        {
            try
            {
                var userCollection = _mongoDbService.GetCollection<UserModel>("Users");
                var result = new List<ChartDataPoint>();
                var endDate = DateTime.Now.Date;
                var startDate = endDate.AddDays(-days);

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var count = await userCollection.CountDocumentsAsync(
                        Builders<UserModel>.Filter.And(
                            Builders<UserModel>.Filter.Gte(u => u.RegisterDate, date),
                            Builders<UserModel>.Filter.Lt(u => u.RegisterDate, date.AddDays(1))
                        ));

                    result.Add(new ChartDataPoint
                    {
                        Date = date,
                        Label = date.ToString("dd/MM"),
                        Value = (int)count
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user growth chart data: {ex.Message}");
                return new List<ChartDataPoint>();
            }
        }

        public async Task<List<ChartDataPoint>> GetActivityChartDataAsync(int days = 7)
        {
            try
            {
                var userCollection = _mongoDbService.GetCollection<UserModel>("Users");
                var result = new List<ChartDataPoint>();
                var endDate = DateTime.Now.Date;
                var startDate = endDate.AddDays(-days);

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var count = await userCollection.CountDocumentsAsync(
                        Builders<UserModel>.Filter.And(
                            Builders<UserModel>.Filter.Gte(u => u.LastLogin, date),
                            Builders<UserModel>.Filter.Lt(u => u.LastLogin, date.AddDays(1))
                        ));

                    result.Add(new ChartDataPoint
                    {
                        Date = date,
                        Label = date.ToString("dd/MM"),
                        Value = (int)count
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting activity chart data: {ex.Message}");
                return new List<ChartDataPoint>();
            }
        }

        public async Task UpdateUserActivityAsync(string userId, string email)
        {
            try
            {
                var activityCollection = _mongoDbService.GetCollection<UserActivityModel>("UserActivities");
                var now = DateTime.Now;

                var filter = Builders<UserActivityModel>.Filter.Eq(a => a.UserId, userId);
                var activity = await activityCollection.Find(filter).FirstOrDefaultAsync();

                if (activity == null)
                {
                    activity = new UserActivityModel
                    {
                        UserId = userId,
                        Email = email,
                        LastLogin = now,
                        LastActivity = now,
                        IsOnline = true,
                        TotalSessions = 1,
                        TotalTimeSpent = TimeSpan.Zero,
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    await activityCollection.InsertOneAsync(activity);
                }
                else
                {
                    var update = Builders<UserActivityModel>.Update
                        .Set(a => a.LastActivity, now)
                        .Set(a => a.IsOnline, true)
                        .Set(a => a.UpdatedAt, now)
                        .Inc(a => a.TotalSessions, 1);

                    await activityCollection.UpdateOneAsync(filter, update);
                }

                // Cập nhật LastLogin trong Users collection
                var userCollection = _mongoDbService.GetCollection<UserModel>("Users");
                var userFilter = Builders<UserModel>.Filter.Eq(u => u.Id, userId);
                var userUpdate = Builders<UserModel>.Update.Set(u => u.LastLogin, now);
                await userCollection.UpdateOneAsync(userFilter, userUpdate);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user activity: {ex.Message}");
            }
        }

        public async Task SaveDailyStatisticsAsync()
        {
            try
            {
                var statistics = await GetDashboardStatisticsAsync();
                var statisticsCollection = _mongoDbService.GetCollection<UserStatisticsModel>("UserStatistics");

                var dailyStats = new UserStatisticsModel
                {
                    Date = DateTime.Now.Date,
                    TotalUsers = statistics.TotalUsers,
                    ActiveUsers = statistics.ActiveUsersToday,
                    NewUsersToday = statistics.NewUsersToday,
                    NewUsersThisWeek = statistics.NewUsersThisWeek,
                    NewUsersThisMonth = statistics.NewUsersThisMonth,
                    UsersByRole = statistics.UsersByRole,
                    UsersByLevel = statistics.UsersByLevel,
                    CreatedAt = DateTime.Now
                };

                // Kiểm tra xem đã có thống kê cho ngày hôm nay chưa
                var existingStats = await statisticsCollection
                    .Find(s => s.Date == DateTime.Now.Date)
                    .FirstOrDefaultAsync();

                if (existingStats == null)
                {
                    await statisticsCollection.InsertOneAsync(dailyStats);
                }
                else
                {
                    var filter = Builders<UserStatisticsModel>.Filter.Eq(s => s.Id, existingStats.Id);
                    await statisticsCollection.ReplaceOneAsync(filter, dailyStats);
                }

                _logger.LogInformation("Daily statistics saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving daily statistics: {ex.Message}");
            }
        }
    }
}
