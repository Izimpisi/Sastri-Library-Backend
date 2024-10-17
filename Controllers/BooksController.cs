using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sastri_Library_Backend.Data;
using Sastri_Library_Backend.Models;

namespace Sastri_Library_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly LibraryAppContext _context;

        // Constructor that accepts DbContext through dependency injection
        public BooksController(LibraryAppContext context)
        {
            _context = context;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Book>>> SearchBooks(
        [FromQuery] string query,
        [FromQuery] string filter)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(filter))
            {
                return BadRequest("Query and filter are required.");
            }

            var books = filter.ToLower() switch
            {
                "isbn" => _context.Books
                    .Where(b => b.ISBN.Contains(query))
                    .Take(5),
                "title" => _context.Books
                    .Where(b => b.Title.Contains(query))
                    .Take(5),
                "author" => _context.Books
                    .Where(b => b.Author.Contains(query))
                    .Take(5),
                _ => Enumerable.Empty<Book>().AsQueryable()
            };

            var result = await books.ToListAsync();

            if (!result.Any())
            {
                return NotFound("No books found.");
            }

            return Ok(result);
        }

        // Example action method to get all books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            return await _context.Books.ToListAsync();
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddBook([FromBody] Book newBook)
        {
            if (newBook == null)
            {
                return BadRequest("Book is null.");
            }

            // Validate the book model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Add the new book to the database
            await _context.Books.AddAsync(newBook);
            await _context.SaveChangesAsync();

            // Return the created book with a 201 status code
            return CreatedAtAction(nameof(GetBookById), new { id = newBook.BookId }, newBook);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }
    }
}
