using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sastri_Library_Backend.Models
{
    public class Student
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Student_ID { set; get; }

        [Required]

        public string Student_FName { set; get; }

        [Required]

        public string Student_LName { set; get; }

     
    }
}
