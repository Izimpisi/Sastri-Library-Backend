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

        public bool Approved { get; set; }

        [StringLength(200, ErrorMessage = "Rejection message cannot exceed 200 characters.")]
        [Display(Name = "Rejection Message")]
        public string RejectionMessage { get; set; }

        [Required(ErrorMessage = "User is required")]
        [ForeignKey("UserID")]
        public int UserID { get; set; }
        public virtual User User { get; set; } 
        
        [Required(ErrorMessage = "Book is required")]
        [ForeignKey("BookId")]
        public int BookId { get; set; }
        public virtual Book Book { get; set; }
    }
}
