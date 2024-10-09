using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sastri_Library_Backend.Models
{
    public class Librarian
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Librarian_Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        public string Lib_firstNam { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
        public string Lib_LastNam { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Lib_EmailAddr { get; set; }

        [Required]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string Lib_PhoneNum { get; set; }
    }
}
