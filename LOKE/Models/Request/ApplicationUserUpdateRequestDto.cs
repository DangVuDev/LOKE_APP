using LOKE.Models.Dto.ApplicationDto;
using LOKE.Models.Model.ApplicationModel;

namespace LOKE.Models.Request
{
    public class ApplicationUserUpdateRequestDto
    {
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? Name { get; set; } = string.Empty;

        // Các trường mở rộng (theo UserModel bên frontend)
        public string? Bio { get; set; }
        public string? Hometown { get; set; }
        public string? Education { get; set; }
        public string? Job { get; set; }
        public string? Company { get; set; }
        public string? Status { get; set; }
        public string? Interests { get; set; }
        public string? ProfileImageUrl { get; set; }

        // Danh sách liên hệ
        public List<ContactInfo>? Contacts { get; set; } = [];
    }
}
