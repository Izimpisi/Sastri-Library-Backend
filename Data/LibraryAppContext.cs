using Microsoft.EntityFrameworkCore;
using Sastri_Library_Backend.Models;
using System.Reflection.Metadata;

namespace Sastri_Library_Backend.Data
{
    public class LibraryAppContext: DbContext
    {
        public LibraryAppContext(DbContextOptions<LibraryAppContext> options)
             : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
    }
}
