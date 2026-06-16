using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.Models
{
    public class Member : BaseEntity
    {
        public int MemberId { get; set; }

        public int UserId { get; set; }
        public Account Account { get; set; } = null!;

        [Required, MaxLength(30)]
        public string MemberCode { get; set; } = string.Empty;

        public DateTime JoinedAt { get; set; }
    }
}
