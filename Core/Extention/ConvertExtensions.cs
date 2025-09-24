using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Core.Extensions
{
    public static class ConvertExtensions
    {
        #region 👉 Primitive Convert

        public static int ToInt(this string value, int defaultValue = 0)
            => int.TryParse(value, out var result) ? result : defaultValue;

        public static long ToLong(this string value, long defaultValue = 0)
            => long.TryParse(value, out var result) ? result : defaultValue;

        public static double ToDouble(this string value, double defaultValue = 0)
            => double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;

        public static decimal ToDecimal(this string value, decimal defaultValue = 0)
            => decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;

        public static bool ToBool(this string value, bool defaultValue = false)
            => bool.TryParse(value, out var result) ? result : defaultValue;

        public static DateTime ToDateTime(this string value, DateTime? defaultValue = null)
            => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
                ? result
                : defaultValue ?? DateTime.MinValue;

        #endregion

        #region 👉 Base64 & Bytes

        public static string ToBase64(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return string.Empty;
            return Convert.ToBase64String(bytes);
        }

        public static byte[] FromBase64ToBytes(this string base64)
        {
            if (string.IsNullOrWhiteSpace(base64)) return Array.Empty<byte>();
            return Convert.FromBase64String(base64);
        }

        public static string ToBase64(this string plainText, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return Convert.ToBase64String(encoding.GetBytes(plainText));
        }

        public static string FromBase64ToString(this string base64, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var bytes = base64.FromBase64ToBytes();
            return encoding.GetString(bytes);
        }

        public static string ToBase64(this Stream stream)
        {
            if (stream == null) return string.Empty;
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return Convert.ToBase64String(ms.ToArray());
        }

        public static Stream FromBase64ToStream(this string base64)
        {
            var bytes = base64.FromBase64ToBytes();
            return new MemoryStream(bytes);
        }

        #endregion

        #region 👉 File Convert

        public static string ToBase64(this FileInfo file)
        {
            if (!file.Exists) throw new FileNotFoundException("File not found", file.Name);
            var bytes = File.ReadAllBytes(file.Name);
            return bytes.ToBase64();
        }

        public static void FromBase64ToFile(this string base64, string outputPath)
        {
            var bytes = base64.FromBase64ToBytes();
            File.WriteAllBytes(outputPath, bytes);
        }

        public static byte[] ToBytes(this FileInfo file)
        {
            if (!file.Exists) throw new FileNotFoundException("File not found", file.Name);
            return File.ReadAllBytes(file.Name);
        }

        public static void ToFile(this byte[] bytes, string outputPath)
        {
            if (bytes == null || bytes.Length == 0) throw new ArgumentNullException(nameof(bytes));
            File.WriteAllBytes(outputPath, bytes);
        }

        #endregion

        #region 👉 TimeZone Convert

        /// <summary>
        /// Convert DateTime sang timezone theo offset giờ (vd: +7 => GMT+7)
        /// </summary>
        public static DateTime ToTime(this DateTime originTime, int offsetHours)
        {
            // Tạo timezone theo offset
            var offset = TimeSpan.FromHours(offsetHours);
            var targetZone = TimeZoneInfo.CreateCustomTimeZone($"UTC{offsetHours:+#;-#;+0}", offset, "Custom Zone", "Custom Zone");

            return TimeZoneInfo.ConvertTime(originTime, targetZone);
        }

        /// <summary>
        /// Convert DateTimeOffset sang timezone khác theo offset giờ
        /// </summary>
        public static DateTimeOffset ToTime(this DateTimeOffset originTime, int offsetHours)
        {
            var offset = TimeSpan.FromHours(offsetHours);
            return originTime.ToOffset(offset);
        }

        #endregion

        // ===== JSON CONVERT =====
        public static string ToJson<T>(this T obj, bool indented = false)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = indented,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(obj, options);
        }

        public static T FromJson<T>(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("Invalid JSON string");

            #pragma warning disable CS8603 // Possible null reference return.
            return JsonSerializer.Deserialize<T>(json);
            #pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
