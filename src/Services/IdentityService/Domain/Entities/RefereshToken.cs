

using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityService.Domain.Entities
    {
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; } = null!;

        // كل التوكنز اللي بتتولد من نفس ال login session بتشترك في نفس ال FamilyId
        // لما نكتشف reuse لتوكن اتلغى، بنلغي الـ family كلها (كل الأجهزة) = criteria 4
        public Guid FamilyId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }

        // بيشاور على التوكن الجديد اللي استبدل ده وقت ال rotation (audit trail)
        public Guid? ReplacedByTokenId { get; set; }

        // Session / Device metadata - criteria 6 (list of active sessions)
        public string? DeviceName { get; set; }
        public string? IpAddress { get; set; }
        public string? Location { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        [NotMapped]
        public bool IsRevoked => RevokedAt != null;

        [NotMapped]
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}


