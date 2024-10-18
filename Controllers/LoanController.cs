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
                    l.DueDate
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
                    l.DueDate
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
