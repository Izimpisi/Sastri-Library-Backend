using Microsoft.EntityFrameworkCore;
using Sastri_Library_Backend.Models;

namespace Sastri_Library_Backend.Data
{
<<<<<<< Updated upstream
    public class LibraryAppContext: IdentityDbContext<Student>
=======
    public class LibraryDbContext : DbContext
>>>>>>> Stashed changes
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
<<<<<<< Updated upstream
=======
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookUpdateLog> BookUpdateLogs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<User> Users { get; set; }
>>>>>>> Stashed changes
    }
}