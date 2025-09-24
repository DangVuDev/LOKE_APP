namespace LOKE.Models.Dto.ApplicationDto
{
    public class ApplicationUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

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
        public List<ContactInfoDto> Contacts { get; set; } = new();

    }
    public class ContactInfoDto
    {
        public string Type { get; set; } = string.Empty;          // Ví dụ: facebook, email, zalo...
        public string DisplayName { get; set; } = string.Empty;   // Tên hiển thị
        public string Value { get; set; } = string.Empty;         // Link hoặc thông tin liên hệ
        public bool IsLink { get; set; } = true;                  // Có phải là link không
        public string Color { get; set; } = "#2196F3";            // Mã màu HEX (thay vì Color object)
        public string? IconUrl { get; set; }                      // Icon riêng nếu có
    }
}
