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

            // Find all copies for the book
            var copies = await _context.BookCopies.Where(c => c.BookId == book.BookId).ToListAsync();


            foreach (var copy in copies)
            {
                var existingReservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.CopyId == copy.CopyId && r.Active && r.ExpireDate <= DateTime.Now);

                if (existingReservation != null)
                {
                    existingReservation.Active = false;
                    existingReservation.Approved = false;
                    existingReservation.Message = "Expired";

                    copy.IsOnReservation = false;

                    await _context.SaveChangesAsync();
                }

            }

            foreach (var copy in copies)
            {
                var activeReservations = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.CopyId == copy.CopyId && r.Active);

                if (activeReservations != null)
                {
                    if (activeReservations.UserID == studentId)
                    {
                        activeReservations.Message = "Used";
                        activeReservations.Active = false;
                        copy.IsOnReservation = false;
                        copy.IsOnLoan = false;
                        await _context.SaveChangesAsync();
                        break;
                    }
                }
            }

            var availableCopy = copies.FirstOrDefault(c => c.IsAvailable);

            if (availableCopy == null)
            {
                return StatusCode(403, "Temporarily Unavailable: No available copies of this book.");
            }

            var newLoan = new Loan
            {
                UserId = studentId,
                BookId = loan.BookId,
                DueDate = loan.Duedate,
                CopyId = availableCopy.CopyId
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

            var copies = await _context.BookCopies.Where(c => c.BookId == loan.BookId).ToListAsync();

            foreach (var copy in copies)
            {
                var existingReservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.BookId == copy.CopyId && r.Active && r.ExpireDate <= DateTime.Now);

                if (existingReservation != null)
                {
                    existingReservation.Active = false;
                    existingReservation.Approved = false;
                    existingReservation.Message = "Expired";

                    copy.IsOnReservation = false;

                    await _context.SaveChangesAsync();
                }

            }

            var availableCopy = copies.FirstOrDefault(c => c.IsAvailable);

            if (availableCopy == null)
            {
                return StatusCode(403, "Temporarily Unavailable: No available copies of this book.");
            }


            loan.Active = true;
            loan.Approved = true;
            loan.Message = "Approved";
            loan.LoanDate = DateTime.Now;
            loan.CopyId = availableCopy.CopyId;
            availableCopy.IsOnLoan = true;


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

            var copy = await _context.BookCopies.FirstOrDefaultAsync(c => c.CopyId == loan.CopyId);

            copy.IsOnLoan = false;
            copy.IsOnReservation = false;

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
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

            // If the user's role is "Student", deny the deletion
            if (user.Role == "Student")
            {
                return Unauthorized("Students are not allowed to delete loans.");
            }

            // Find the loan by ID
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
            {
                return NotFound("Loan not found.");
            }

            // If the loan is still active, proceed with deletion
            if (loan.Active)
            {
                return StatusCode(403, "Cannot delete an active loan.");
            }

            // Check if there is a bill associated with the loan
            var existingBill = await _context.Bills.FirstOrDefaultAsync(b => b.LoanId == loan.Id);
            if (existingBill != null && !existingBill.Settled)
            {
                return StatusCode(403, "Cannot delete a loan with an associated unsettled bill.");
            }

            // Proceed with deleting the loan
            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content on success
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
