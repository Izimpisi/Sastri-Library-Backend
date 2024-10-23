using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Sastri_Library_Backend.Models
{
    public class BookCopy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CopyId { get; set; }

        [Required] 
        [ForeignKey("Book")]
        public int BookId { get; set; }

        public Book Book { get; set; }

        [Required] 
        public bool IsOnLoan { get; set; }

        [Required] 
        public bool IsOnReservation { get; set; }

        public bool IsAvailable => !IsOnLoan && !IsOnReservation;

        public BookCopy()
        {
            IsOnLoan = false;
            IsOnReservation = false;
        }
    }
}
