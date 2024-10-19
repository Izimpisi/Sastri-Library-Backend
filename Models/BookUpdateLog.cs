using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sastri_Library_Backend.Models
{
    public class BookUpdateLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Log_ID { get; set; }

        [Required(ErrorMessage = "Action type is required.")]
        [StringLength(50, ErrorMessage = "Action type cannot exceed 50 characters.")]
        public string ActionType { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Timespan is required.")]
        [StringLength(100, ErrorMessage = "Timespan cannot exceed 100 characters.")]
        public string Timespan { get; set; }

        [ForeignKey(nameof(User))]
        public virtual string UserId { get; set; }

        public virtual User User { get; set; }
    }
}
