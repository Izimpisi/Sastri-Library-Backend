using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sastri_Library_Backend.Data;
using Sastri_Library_Backend.Models;

namespace Sastri_Library_Backend.Controllers
{
    [Authorize(Roles = "Student")] // Only allow access for students
    [Route("api/[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly LibraryAppContext _context;

        public LoanController(LibraryAppContext context)
        {
            _context = context;
        }

        // GET: api/loan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loan>>> GetLoans()
        {
            var loans = await _context.Loans
                .Include(l => l.Student)
                .Include(l => l.Book)
                .ToListAsync();

            return Ok(loans);
        }

        // GET: api/loan/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Loan>> GetLoan(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Student)
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null)
            {
                return NotFound();
            }

            return Ok(loan);
        }

        // POST: api/loan
        [HttpPost]
        public async Task<ActionResult<Loan>> CreateLoan([FromBody] Loan loan)
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

            loan.StudentId = studentId; // Set StudentId from token

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, loan);
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
    }
}
