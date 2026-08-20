using System.ComponentModel.DataAnnotations;

namespace HasbitFlowApi.DTOs.Auth
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
