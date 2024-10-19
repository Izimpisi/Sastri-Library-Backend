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
    public class BillController : ControllerBase
    {
        private readonly LibraryAppContext _context;

        public BillController(LibraryAppContext context)
        {
            _context = context;
        }

        // GET: api/bill
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllBills()
        {
            var bills = await _context.Bills.Select(b => new
            {
                b.Id,
                b.CurrentAmountOwing,
                b.BillPaidAmount,
                b.DaysOutstanding
            }).ToListAsync();

            return Ok(bills);
        }

        [HttpGet("/students")]
        public async Task<ActionResult<IEnumerable<object>>> GetBills()
        {
            // Get the user ID from JWT token claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            var bills = await _context.Bills.Select(b => new
            {
                b.Id,
                b.CurrentAmountOwing,
                b.BillPaidAmount,
                b.DaysOutstanding,
                b.UserId
            }).Where(l => l.UserId == userId).ToListAsync();

            return Ok(bills);
        }


        // GET: api/bill/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Bill>> GetBill(int id)
        {
            var bill = await _context.Bills.FindAsync(id);

            if (bill == null)
            {
                return NotFound("Bill not found.");
            }

            return Ok(bill);
        }

        // POST: api/bill
        [HttpPost]
        public async Task<ActionResult<Bill>> CreateBill([FromBody] BillDto billDto)
        {
            if (billDto == null)
            {
                return BadRequest("Bill data is required.");
            }

            var newBill = new Bill
            {
                CurrentAmountOwing = billDto.CurrentAmountOwing,
                BillPaidAmount = billDto.BillPaidAmount,
               
            };

            _context.Bills.Add(newBill);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBill), new { id = newBill.Id }, newBill);
        }

        [HttpGet("{id}/payments")]
        public async Task<ActionResult<IEnumerable<object>>> GetPaymentsByBillId(int id)
        {
            // Find the bill with the given ID
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
            {
                return NotFound("Bill not found.");
            }

            // Retrieve payments associated with this bill
            var payments = await _context.Payments
                .Where(p => p.BillId == id)
                .Select(p => new
                {
                    p.Id,
                    p.AmountPaid,
                    p.PaymentDate
                })
                .ToListAsync();

            if (payments == null || !payments.Any())
            {
                return NotFound("No payments found for this bill.");
            }

            return Ok(payments);
        }

        // PUT: api/bill/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBill(int id, [FromBody] BillDto billDto)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
            {
                return NotFound("Bill not found.");
            }

            bill.CurrentAmountOwing = billDto.CurrentAmountOwing;
            bill.BillPaidAmount = billDto.BillPaidAmount;
            

            _context.Entry(bill).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BillExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/bill/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBill(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
            {
                return NotFound("Bill not found.");
            }

            _context.Bills.Remove(bill);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BillExists(int id)
        {
            return _context.Bills.Any(e => e.Id == id);
        }

        public class BillDto
        {
            public decimal CurrentAmountOwing { get; set; }
            public decimal BillPaidAmount { get; set; }
            public DateTime? PaidDate { get; set; }
        }
    }
}
