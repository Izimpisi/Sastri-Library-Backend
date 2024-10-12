using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Sastri_Library_Backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Sastri_Library_Backend.Models
{
    public class Student : IdentityUser
    {
        [Required]
        public string StudentIdNumber { set; get; }

        [Required]

        public string FirstName { set; get; }

        [Required]

        public string LastName { set; get; }

    }
}
