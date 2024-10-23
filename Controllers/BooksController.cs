using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sastri_Library_Backend.Data;
using Sastri_Library_Backend.Models;
using System.Security.Claims;

namespace Sastri_Library_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlatBookDto>>> GetBooks()
        {
            var randomBooks = await _context.Books
                .OrderBy(b => EF.Functions.Random())
                .Take(30)
                .ToListAsync();

            var bookCopies = await _context.BookCopies.ToListAsync();

            var result = randomBooks.Select(book => new FlatBookDto
            {
                BookId = book.BookId,
                Title = book.Title,
                DatePublished = book.Date_Published, // Note: Adjust property name if necessary
                Description = book.Description,
                Author = book.Author,
                ISBN = book.ISBN,
                CopyCount = bookCopies.Count(copy => copy.BookId == book.BookId)
            }).ToList();

            return Ok(result);
        }


        [HttpGet("search/list")]
        public async Task<ActionResult<IEnumerable<FlatBookDto>>> SearchAllBooks(
    [FromQuery] string query,
    [FromQuery] string filter)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(filter))
            {
                return BadRequest("Query and filter are required.");
            }

            IQueryable<FlatBookDto> books = filter.ToLower() switch
            {
                "isbn" => _context.Books
                    .Where(b => b.ISBN.Contains(query))
                    .Select(b => new FlatBookDto
                    {
                        BookId = b.BookId,
                        Title = b.Title,
                        DatePublished = b.Date_Published, 
                        Description = b.Description,
                        Author = b.Author,
                        ISBN = b.ISBN,
                        CopyCount = _context.BookCopies.Count(c => c.BookId == b.BookId)
                    }),

                "title" => _context.Books
                    .Where(b => b.Title.Contains(query))
                    .Select(b => new FlatBookDto
                    {
                        BookId = b.BookId,
                        Title = b.Title,
                        DatePublished = b.Date_Published, 
                        Description = b.Description,
                        Author = b.Author,
                        ISBN = b.ISBN,
                        CopyCount = _context.BookCopies.Count(c => c.BookId == b.BookId)
                    }),

                "author" => _context.Books
                    .Where(b => b.Author.Contains(query))
                    .Select(b => new FlatBookDto
                    {
                        BookId = b.BookId,
                        Title = b.Title,
                        DatePublished = b.Date_Published, 
                        Author = b.Author,
                        ISBN = b.ISBN,
                        CopyCount = _context.BookCopies.Count(c => c.BookId == b.BookId)
                    }),

                _ => Enumerable.Empty<FlatBookDto>().AsQueryable()
            };

            if (books == null || !books.Any())
            {
                return NotFound("No books found.");
            }

            var result = await books.Take(15).ToListAsync();

            return Ok(result);
        }


        [HttpPost("add")]
        public async Task<IActionResult> AddBook([FromBody] Book newBook)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid user ID.");
            }

            if (newBook == null)
            {
                return BadRequest("Book is null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _context.Books.AddAsync(newBook);

            try
            {
                await _context.SaveChangesAsync();

                var log = new BookUpdateLog
                {
                    ActionType = "Add",
                    Description = $"Added a new book: {newBook.Title} - {newBook.ISBN}",
                    ActionTime = DateTime.UtcNow, 
                    UserId = userId 
                };

                _context.BookUpdateLogs.Add(log);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetBookById), new { id = newBook.BookId }, newBook);
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
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

        public class FlatBookDto
        {
            public int BookId { get; set; }
            public string Title { get; set; }
            public string DatePublished { get; set; }
            public string Description { get; set; }
            public string Author { get; set; }
            public string ISBN { get; set; }
            public int CopyCount { get; set; } // Add CopyCount here
        }


    }
}
