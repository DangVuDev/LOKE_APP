using System;

namespace Core.Model.Settings
{
    /// <summary>
    /// Represents the configuration settings for the email service.
    /// </summary>
    public class MailSettings
    {
        /// <summary>
        /// Gets or sets the SMTP server address (e.g., "smtp.gmail.com").
        /// </summary>
        public string SmtpServer { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the port number for the SMTP server (e.g., 587).
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the sender's display name (e.g., "Your App").
        /// </summary>
        public string SenderName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sender's email address (e.g., "your-email@gmail.com").
        /// </summary>
        public string SenderEmail { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the username for SMTP authentication.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for SMTP authentication.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether SSL/TLS is enabled for the SMTP connection.
        /// Defaults to true.
        /// </summary>
        public bool EnableSsl { get; set; } = true;
    }
}