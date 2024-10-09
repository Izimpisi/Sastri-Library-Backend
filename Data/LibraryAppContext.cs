using Microsoft.EntityFrameworkCore;
using Sastri_Library_Backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace Sastri_Library_Backend.Data
{
    public class LibraryAppContext: IdentityDbContext<Student>
    {
        public LibraryAppContext(DbContextOptions<LibraryAppContext> options)
             : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
    }
}
