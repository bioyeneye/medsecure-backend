namespace MedSecureSystem.Infrastructure.Options
{
    public class JwtTokenOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public bool ValidateAudience { get; set; }
    }
}
