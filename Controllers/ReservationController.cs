using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sastri_Library_Backend.Data;
using Sastri_Library_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sastri_Library_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class ReservationController : ControllerBase
    {
        private readonly LibraryAppContext _context;

        public ReservationController(LibraryAppContext context)
        {
            _context = context;
        }

        // POST: api/reservation
        [HttpPost]
        public async Task<ActionResult<Reservation>> CreateReservation([FromBody] ReservvationDto reservation)
        {
            if (reservation == null || reservation.BookId <= 0)
            {
                return BadRequest("Reservation data is required.");
            }

            // Get UserId from token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid user token.");
            }

            var book = await _context.Books.FindAsync(reservation.BookId);

            if (book == null)
            {
                return NotFound("Book not found.");
            }

            if (book.IsOnLoan)
            {
                return StatusCode(403, "The book is already on loan.");
            }

            if (book.IsOnReservation)
            {
                return StatusCode(403, "The book is already reserved.");
            }
           
            // Create reservation
            Reservation newReservation = new Reservation();
            newReservation.BookId = reservation.BookId;
            newReservation.UserID = userId;


            _context.Reservations.Add(newReservation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReservation), new { id = newReservation.Id }, reservation);
        }

        // GET: api/reservation/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User) // Assuming you have a User navigation property
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return Ok(reservation);
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservationsByUser()
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userID))
            {
                return Unauthorized("Invalid user token.");
            }

            var reservations = await _context.Reservations
            .Where(r => r.UserID == userID)
            .Select(r => new
            {
                r.Id,
                r.ReservationDate,
                r.ExpireDate,
                r.Approved,
                r.Active,
                r.Message,

                // Flattening the book details
                r.Book.BookId,
                r.Book.Title,
                r.Book.Author,
                r.Book.ISBN,
                r.Book.Description,
                r.Book.Date_Published,
                r.Book.IsOnLoan,
                r.Book.IsOnReservation
            })
      .ToListAsync();


            return Ok(reservations);
        }

        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetAllReservations()
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userID))
            {
                return Unauthorized("Invalid user token.");
            }

            var reservations = await _context.Reservations
            .Select(r => new
            {
                r.User.FirstName, 
                r.User.LastName,
                r.Id,
                r.ReservationDate,
                r.ExpireDate,
                r.Approved,
                r.Active,
                r.Message,

                // Flattening the book details
                r.Book.BookId,
                r.Book.Title,
                r.Book.Author,
                r.Book.ISBN,
                r.Book.Description,
                r.Book.Date_Published,
                r.Book.IsOnLoan,
                r.Book.IsOnReservation
            }).ToListAsync();

            return Ok(reservations);
        }


        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveReservation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid user ID.");
            }

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
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound("Loan not found.");
            }

            var book = await _context.Books.FindAsync(reservation.BookId);
            if (book == null)
            {
                return NotFound("Book not found.");
            }

            if (book.IsOnLoan)
            {
                return StatusCode(403, "The book is already on loan.");
            }
            if (book.IsOnReservation)
            {
                return StatusCode(403, "The book is reserved.");
            }

            book.IsOnReservation = true;
            reservation.Active = true;
            reservation.Approved = true;
            reservation.Message = "Approved";

            // Save changes to the database
            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content on success
        }

        public class ReservvationDto
        {
            public int BookId { get; set; }
        }
    }
}