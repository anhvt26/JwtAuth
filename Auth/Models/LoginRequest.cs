using System.ComponentModel.DataAnnotations;

namespace JwtAuth.Identity.Models
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        public DeviceInfo Device { get; set; } = new DeviceInfo();
    }
}
