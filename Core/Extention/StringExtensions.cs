using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Extention
{
    public static class StringExtensions
    {
        /// <summary>
        /// Check string có null hoặc toàn khoảng trắng
        /// </summary>
        public static bool IsNullOrEmpty(this string? value) =>
            string.IsNullOrEmpty(value);

        public static bool IsNullOrWhiteSpace(this string? value) =>
            string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// Viết hoa chữ cái đầu
        /// </summary>
        public static string CapitalizeFirst(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            return char.ToUpper(value[0]) + value.Substring(1);
        }

        /// <summary>
        /// Chỉ giữ chữ và số
        /// </summary>
        public static string OnlyAlphanumeric(this string value)
        {
            if (value == null) return string.Empty;
            return Regex.Replace(value, "[^a-zA-Z0-9]", "");
        }

        /// <summary>
        /// Convert sang int (safe)
        /// </summary>
        public static int ToInt(this string value, int defaultValue = 0)
        {
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Convert sang DateTime (safe)
        /// </summary>
        public static DateTime? ToDateTime(this string value, string? format = null)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (format == null)
            {
                return DateTime.TryParse(value, out var date) ? date : null;
            }
            else
            {
                return DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var date) ? date : null;
            }
        }

        /// <summary>
        /// Hash SHA256
        /// </summary>
        public static string ToSha256(this string value)
        {
            if (value == null) return string.Empty;
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(value);
            var hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Rút gọn string (max length + ...)
        /// </summary>
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// Snake_case sang PascalCase
        /// </summary>
        public static string ToPascalCase(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var words = value.Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();
            foreach (var w in words)
            {
                result.Append(char.ToUpper(w[0]) + w.Substring(1).ToLower());
            }
            return result.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsEmail(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Regex pattern chuẩn cho email
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// PascalCase sang snake_case
        /// </summary>
        public static string ToSnakeCase(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return Regex.Replace(value, "([a-z])([A-Z])", "$1_$2").ToLower();
        }

        /// <summary>
        /// Base64 encode
        /// </summary>
        public static string ToBase64(this string value)
        {
            if (value == null) return string.Empty;
            var bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Base64 decode
        /// </summary>
        public static string FromBase64(this string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
