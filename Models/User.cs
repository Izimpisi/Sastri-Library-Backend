using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Sastri_Library_Backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Sastri_Library_Backend.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string UserIdNumber { set; get; }

        [Required]

        public string FirstName { set; get; }

        [Required]
        public string LastName { set; get; }

        [Required]
        public string Role { get; set; }

    }
}
