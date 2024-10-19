using Microsoft.EntityFrameworkCore;
using Sastri_Library_Backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Sastri_Library_Backend.Data
{
    public class LibraryAppContext : IdentityDbContext<User>
    {
        public LibraryAppContext(DbContextOptions<LibraryAppContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookUpdateLog> BookUpdateLogs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Bill> Bills { get; set; }
    }
}