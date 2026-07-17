namespace AuthService.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        // FK to AspNetUsers.Id
        public string UserId { get; set; } = default!;
        public User User { get; set; } = default!;

        public string TokenHash { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}
