namespace MedSecureSystem.Infrastructure.Options
{
    public class EmailSettings
    {
        public string GmailEmail { get; set; }
        public string GmailAppPassword { get; set; }
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
    }
}
