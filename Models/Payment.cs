using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sastri_Library_Backend.Models
{
    public class Payment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Payment_ID { get; set; }

        [Required]
        public double Amount { get; set; }


        [Required]
        public DateTime Payment_Date { get; set; }

        [Required]
        public string Payment_Methond { get; set; }

        public virtual Student Student { get; set; }
        public virtual string Student_ID {get;set;}

        public virtual Librarian Librarian { get; set; }
        public virtual string Librarian_Id { get; set; }
    }
}
