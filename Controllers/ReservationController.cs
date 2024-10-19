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

[Route("api/[controller]")]
[ApiController]
public class ReservationController : ControllerBase
{
    private readonly LibraryAppContext _context;

    public ReservationController(LibraryAppContext context)
    {
        _context = context;
    }

    // POST: api/reservation
    [HttpPost]
    public async Task<ActionResult<Reservation>> CreateReservation([FromBody] Reservation reservation)
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

        // Check if the book exists
        var bookExists = await _context.Books.AnyAsync(b => b.BookId == reservation.BookId);
        if (!bookExists)
        {
            return NotFound("The specified book does not exist.");
        }
        
        // Create reservation
        reservation.UserID = userId; // Set UserId from token
        reservation.ReservationDate = DateTime.UtcNow;
        reservation.Approved = false; // Initially set to false

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
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
            .Include(r => r.Book)
            .ToListAsync();

        return Ok(reservations);
    }

    // PUT: api/reservation/approve/{id}
    [HttpPut("approve/{id}")]
    public async Task<IActionResult> ApproveReservation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }

        reservation.Approved = true; // Approve the reservation
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
