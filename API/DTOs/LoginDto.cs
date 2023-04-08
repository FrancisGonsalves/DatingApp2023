using System.ComponentModel.DataAnnotations;
namespace API.DTOs
{
    public class LoginDto
    {
        string userName;
        [Required]
        public string UserName { 
            get {
                return userName;
            }
            set {
                userName = value.ToLower();
            }
        }
        [Required]
        public string Password { get; set; }
    }
}