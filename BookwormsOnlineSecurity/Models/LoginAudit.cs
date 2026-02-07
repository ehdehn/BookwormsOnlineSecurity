namespace BookwormsOnlineSecurity.Models
{
    public class LoginAudit
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool Success { get; set; }
        public bool IsLogout { get; set; }
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
    }
}
