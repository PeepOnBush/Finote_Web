using Finote_Web.Models.Data;

namespace Finote_Web.Repositories.Logging
{
    public class ActivityLogRepository : IActivityLogRepository
    {
        private readonly FinoteDbContext _context;

        public ActivityLogRepository(FinoteDbContext context)
        {
            _context = context;
        }

        public async Task LogActivityAsync(string userId, string action)
        {
            var log = new ActivityLog
            {
                UserId = userId,
                Action = action,
                Timestamp = DateTime.UtcNow // Use UTC for server time consistency
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}