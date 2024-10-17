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
        public DateTime LoanDate { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Please enter a valid date.")]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Please enter a valid date.")]
        [Display(Name = "Return Date")]
        public DateTime? ReturnDate { get; set; }

        public bool Approved { get; set; }

        [StringLength(200, ErrorMessage = "Rejection message cannot exceed 200 characters.")]
        [Display(Name = "Rejection Message")]
        public string RejectionMessage { get; set; }

        [Required(ErrorMessage = "A student is required.")]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }

        [Required(ErrorMessage = "A student is required.")]
        public virtual User User { get; set; }

        [Required(ErrorMessage = "A book is required.")]
        [ForeignKey(nameof(Book))]
        public int BookId { get; set; }

        [Required(ErrorMessage = "A book is required.")]
        public virtual Book Book { get; set; }
        public Loan()
        {
            Approved = false;
            RejectionMessage = "Pending";
        }
    }
}
