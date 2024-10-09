using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Sastri_Library_Backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sastri_Library_Backend.Models
{
    public class Student : IdentityUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Student_ID { set; get; }

        [Required]

        public string Student_FName { set; get; }

        [Required]

        public string Student_LName { set; get; }

    }
}
