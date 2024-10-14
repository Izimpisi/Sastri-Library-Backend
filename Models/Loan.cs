using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sastri_Library_Backend.Models
{
    public class Loan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Loan date is required")]
        [DataType(DataType.Date, ErrorMessage = "Please enter a valid date")]
        public DateTime LoanDate { get; set; }

        [Required(ErrorMessage = "Due date is required")]
        [DataType(DataType.Date, ErrorMessage = "Please enter a valid date")]
        public DateTime DueDate { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Please enter a valid date")]
        public DateTime? ReturnDate { get; set; }

        public bool Approved { get; set; }

        [StringLength(200)]
        public string RejectionMessage { get; set; }

        [Required]
        [ForeignKey("StudentId")]
        public string StudentId { get; set; }

        [Required(ErrorMessage = "A student is required")]
        public virtual User Student { get; set; } 
        
        [Required]
        [ForeignKey("BookId")]
        public int BookId { get; set; }

        [Required(ErrorMessage = "A student is required")]
        public virtual Book Book { get; set; }
    }
}

