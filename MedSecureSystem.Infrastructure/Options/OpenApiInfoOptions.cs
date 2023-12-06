namespace MedSecureSystem.Infrastructure.Options
{
    using Microsoft.OpenApi.Models;

    public class OpenApiInfoOptions
    {
        public OpenApiInfoOptions()
        {

        }
        public OpenApiInfoOptions(string title, string version, string description, ContactInfo contact)
        {
            Title = title;
            Version = version;
            Description = description;
            Contact = contact;
        }

        public string Title { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public ContactInfo Contact { get; set; }
        // Other properties as needed...

        public OpenApiInfo ToOpenApiInfo()
        {
            return new OpenApiInfo
            {
                Title = Title,
                Version = Version,
                Description = Description,
                Contact = Contact?.ToContact()
                // Map other properties to OpenApiInfo as needed...
            };
        }
    }

    public class ContactInfo
    {
        public ContactInfo()
        {

        }
        public ContactInfo(string name, string email, string url)
        {
            Name = name;
            Email = email;
            Url = url;
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }

        public OpenApiContact ToContact()
        {
            return new OpenApiContact
            {
                Name = Name,
                Email = Email,
                Url = new Uri(Url)
            };
        }
    }
}
