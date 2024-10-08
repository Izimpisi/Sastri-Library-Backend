using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sastri_Library_Backend.Models
{
    public class Librarian
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Librarian_Id { get; set; }

        [Required]
         public string Lib_firstNam { get; set; }

        [Required]
        public string Lib_LastNam { get; set; }

        [Required]
        public string Lib_EmailAddr { get; set; }

        [Required]
        public string Lib_PhoneNum { get;set; }
    }
}
