namespace Finote_API.Services.SendEmail
{
    public interface IEmailSenderService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
