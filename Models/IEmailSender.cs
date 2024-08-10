namespace IdentityApp.Models
{
    public interface IEmailSender
    {
        Task SendEmainAsync(string email, string subject, string message);
    }
}
