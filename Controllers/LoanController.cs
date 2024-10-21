using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sastri_Library_Backend.Data;
using Sastri_Library_Backend.Models;

namespace Sastri_Library_Backend.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly LibraryAppContext _context;

        public LoanController(LibraryAppContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetLoans()
        {
            // Get the user ID from JWT token claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid user ID.");
            }

            // Fetch the user's role from the database
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Check if the user has the 'Admin' role
            if (user.Role == "Admin")
            {
                return Forbid("You are not authorized to view loans.");
            }


            try
            {
                var loans = await _context.Loans.Select(l => new
                {
                    LoanId = l.Id,
                    l.UserId,
                    l.Book.Title,
                    l.Book.ISBN,
                    l.Book.Author,
                    l.LoanDate,
                    l.ReturnDate,
                    l.DueDate,
                    l.Message,
                    l.Approved,
                    l.Active
                }).Where(l => l.UserId == userId)
            .ToListAsync();

                return Ok(loans);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }

        }

        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllLoans()
        {
            // Get the user ID from JWT token claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid user ID.");
            }

            // Fetch the user's role from the database
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Check if the user has the 'Admin' role
            if (user.Role == "Student")
            {
                return Forbid("You are not authorized to view loans.");
            }


            try
            {
                var loans = await _context.Loans.Select(l => new
                {
                    LoanId = l.Id,
                    l.UserId,
                    l.User.FirstName,
                    l.User.LastName,
                    l.Book.Title,
                    l.Book.ISBN,
                    l.Book.Author,
                    l.LoanDate,
                    l.ReturnDate,
                    l.DueDate,
                    l.Message,
                    l.Approved,
                    l.Active
                })
            .ToListAsync();

                return Ok(loans);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }

        }


        // GET: api/loan/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Loan>> GetLoan(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.User)
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null)
            {
                return NotFound();
            }

            return Ok(loan);
        }


        [HttpPost]
        public async Task<ActionResult<Loan>> CreateLoan([FromBody] LoanDto loan)
        {
            if (loan == null)
            {
                return BadRequest("Loan data is required.");
            }

            // Get StudentId from token
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(studentId))
            {
                return Unauthorized("Invalid student token.");
            }

            var book = await _context.Books.FindAsync(loan.BookId);
            if (book == null)
            {
                return NotFound("Book not found.");
            }

            if (book.IsOnReservation)
            {
                // Find the existing reservation for this book
                var existingReservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.BookId == book.BookId && r.Active);

                if (existingReservation != null && existingReservation.ExpireDate <= DateTime.Now)
                {
                    // Expire the existing reservation
                    book.IsOnReservation = false;
                    existingReservation.Active = false;
                    existingReservation.Approved = false;
                    existingReservation.Message = "Expired";

                    // Save the changes to the database
                    await _context.SaveChangesAsync();
                }
            }

            if (book.IsOnLoan)
            {
                return StatusCode(403, "Temporarily Unavailable: This book is out on Loan.");
            }

            if (book.IsOnReservation)
            {
                var existingReservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.BookId == book.BookId && r.Active);

                if (existingReservation != null)
                {
                    if (existingReservation.UserID == studentId)
                    {
                        existingReservation.Message = "Used";
                        existingReservation.Active = false;
                        book.IsOnReservation = false;
                    }
                    else
                    {
                        return StatusCode(403, "The book is already reserved.");
                    }
                }
            }


            var newLoan = new Loan
            {
                UserId = studentId,
                BookId = loan.BookId,
                DueDate = loan.Duedate
            };

            _context.Loans.Add(newLoan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLoan), new { id = newLoan.Id }, loan);
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveLoan(int id)
        {
            // Get the user ID from JWT token claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid user ID.");
            }

            // Fetch the user's role from the database
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Check if the user has the 'Admin' role
            if (user.Role != "Admin")
            {
                return Forbid("You are not authorized to approve loans.");
            }

            // Find the loan to approve
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
            {
                return NotFound("Loan not found.");
            }

            var book = await _context.Books.FindAsync(loan.BookId);

            if (book.IsOnReservation)
            {
                // Find the existing reservation for this book
                var existingReservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.BookId == book.BookId && r.Active);

                if (existingReservation != null && existingReservation.ExpireDate <= DateTime.Now)
                {
                    // Expire the existing reservation
                    book.IsOnReservation = false;
                    existingReservation.Active = false;
                    existingReservation.Approved = false;
                    existingReservation.Message = "Expired";

                    // Save the changes to the database
                    await _context.SaveChangesAsync();
                }
            }

            if (book == null)
            {
                return NotFound("Book not found.");
            }
            if (book.IsOnLoan)
            {
                return StatusCode(403, "Temporarily Unavailable: This book is out on Loan.");
            }

            if (book.IsOnReservation)
            {
                var existingReservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.BookId == book.BookId && r.Active);

                if (existingReservation != null)
                {
                    if (existingReservation.UserID == userId)
                    {
                        existingReservation.Message = "Used";
                        existingReservation.Active = false;
                        book.IsOnReservation = false;
                    }
                    else
                    {
                        return StatusCode(403, "The book is already reserved.");
                    }
                }
            }

            book.IsOnLoan = true;
            loan.Active = true;
            loan.Approved = true;
            loan.Message = "Approved";
            loan.LoanDate = DateTime.Now;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content on success
        }

        [HttpPost("{id}/return")]
        public async Task<IActionResult> ReturnLoan(int id)
        {
            // Find the loan by ID
            var loan = await _context.Loans.Include(l => l.Book).FirstOrDefaultAsync(l => l.Id == id);
            if (loan == null)
            {
                return NotFound("Loan not found.");
            }

            // Find the associated book and set IsOnLoan to false
            var book = loan.Book;
            if (book == null)
            {
                return NotFound("Book not found.");
            }
            book.IsOnLoan = false;
            book.IsOnReservation = false;

            // Set the loan's ReturnDate to today
            loan.ReturnDate = DateTime.Now;
            loan.Active = false;
            loan.Message = "Returned";

            // Check if the ReturnDate is after the DueDate (i.e., the loan is overdue)
            if (loan.ReturnDate > loan.DueDate)
            {
                TimeSpan difference = loan.ReturnDate.Value - loan.DueDate.Value;

                // Get the total number of days from the TimeSpan
                int overdueDays = Convert.ToInt32(difference.TotalDays);

                // Check if a bill already exists for this loan
                var existingBill = await _context.Bills.FirstOrDefaultAsync(b => b.LoanId == loan.Id);

                if (existingBill == null)
                {
                    // Create a new bill if none exists
                    var newBill = new Bill
                    {
                        LoanId = loan.Id,
                        UserId = loan.UserId,
                        DueDate = loan.DueDate,
                        CurrentAmountOwing = overdueDays * 3, // 3 rands per day
                        BillPaidAmount = 0,
                        DaysOverdue = overdueDays
                    };
                    _context.Bills.Add(newBill);
                }
                else
                {
                    // Update the existing bill with overdue details
                    existingBill.DaysOverdue = overdueDays;
                    existingBill.CurrentAmountOwing = overdueDays * 3; // 3 rands per day
                }
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content on success
        }

        // PUT: api/loan/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLoan(int id, [FromBody] Loan loan)
        {
            if (id != loan.Id)
            {
                return BadRequest();
            }

            _context.Entry(loan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoanExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/loan/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
            {
                return NotFound();
            }

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LoanExists(int id)
        {
            return _context.Loans.Any(e => e.Id == id);
        }

        public class LoanDto
        {
            public int BookId { get; set; }
            public DateTime Duedate { get; set; }
        }
    }
}
