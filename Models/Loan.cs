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

        [DataType(DataType.Date, ErrorMessage = "Please enter a valid date.")]
        public DateTime? LoanDate { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Please enter a valid date.")]
       
        public DateTime DueDate { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Please enter a valid date.")]
        
        public DateTime? ReturnDate { get; set; }

        public bool Approved { get; set; }

        [StringLength(200, ErrorMessage = "Rejection message cannot exceed 200 characters.")]
        [Display(Name = "Rejection Message")]
        public string? RejectionMessage { get; set; }

        [ForeignKey(nameof(User))]
        public string UserId { get; set; }

        public virtual User? User { get; set; }

        [ForeignKey(nameof(Book))]
        public int BookId { get; set; }

        public virtual Book? Book { get; set; }
        public Loan()
        {
            Approved = false;
            RejectionMessage = "Pending";
        }
    }
}
