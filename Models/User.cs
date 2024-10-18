using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Sastri_Library_Backend.Models
{
    public class User : IdentityUser
    {
        [Required(ErrorMessage = "User ID Number is required.")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "User ID Number must be between 5 and 20 characters.")]
        public string UserIdNumber { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "First Name can only contain letters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "Last Name can only contain letters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        [StringLength(20, ErrorMessage = "Role cannot exceed 20 characters.")]
        [RegularExpression(@"^(Librarian|Student|Admin)$",
            ErrorMessage = "Role must be either 'Librarian', 'Student', or 'Admin'.")]
        public string Role { get; set; }
    }
}
