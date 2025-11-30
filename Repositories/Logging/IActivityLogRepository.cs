namespace Finote_Web.Repositories.Logging
{
    public interface IActivityLogRepository
    {
        Task LogActivityAsync(string userId, string action);
    }
}