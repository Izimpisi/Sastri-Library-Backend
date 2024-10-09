using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sastri_Library_Backend.Models
{
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Reservation date is required")]
        [DataType(DataType.Date, ErrorMessage = "Please enter a valid date")]
        public DateTime ReservationDate { get; set; }

        [Required(ErrorMessage = "Expiration date is required")]
        [DataType(DataType.Date, ErrorMessage = "Please enter a valid date")]
        public DateTime ExpireDate { get; set; }

        [Required(ErrorMessage = "Librarian is required")]
        [ForeignKey("LibrarianId")]
        public int LibrarianId { get; set; }
        public virtual Librarian Librarian { get; set; }
    }
}
