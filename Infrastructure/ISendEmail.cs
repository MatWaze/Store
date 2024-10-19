namespace Store.Infrastructure
{
    public interface ISendEmail
    {
        public Task<string> SendEmailAsync(string emailName, string subject, string body);
    }
}
