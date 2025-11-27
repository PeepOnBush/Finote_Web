using System.ComponentModel.DataAnnotations;

namespace Finote_Web.Models.Data
{
    public class UserInfomation
    {
        [Key]
        public required string UserInfomationId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;

        public virtual Users? User { get; set; }
    }
}
