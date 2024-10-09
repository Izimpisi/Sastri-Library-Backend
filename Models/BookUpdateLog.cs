using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sastri_Library_Backend.Models
{
    public class BookUpdateLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
         public string Log_ID { get; set; }

        [Required]
        public string ActionType { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Timespan { get; set; }

        public virtual Librarian Librarian { get; set; }
        
        public virtual string Librarian_Id { get; set; }
    }
}
