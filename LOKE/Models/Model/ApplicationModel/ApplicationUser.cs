using Core.Model.Aplication;

namespace LOKE.Models.Model.ApplicationModel
{
    public class ApplicationUser : BaseUser
    {
        // Các thông tin cá nhân cơ bản
        public string Name { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string Hometown { get; set; } = string.Empty;
        public string Education { get; set; } = string.Empty;
        public string Job { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Interests { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
        public List<ContactInfo>? Contacts { get; set; } = new();
    }

    public class ContactInfo
    {
        public string Type { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty; 
        public string Value { get; set; } = string.Empty; 
        public bool IsLink { get; set; } = true;
        public string Color { get; set; } = "#2196F3"; 
        public string? IconUrl { get; set; } 
    }
}
