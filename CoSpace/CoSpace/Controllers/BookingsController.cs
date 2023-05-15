using CoSpace.Data;
using CoSpace.Data.Entities;
using CoSpace.Helpers;
using CoSpace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoSpace.Controllers
{
    public class BookingsController : Controller
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public BookingsController(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            return _context.Bookings != null ?
                        View(await _context.Bookings.ToListAsync()) :
                        Problem("Entity set 'DataContext.Bookings'  is null.");
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Bookings == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }


        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                booking.BookingState = Enums.BookingState.Confirmada;
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Confirm(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Booking booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.BookingState != Enums.BookingState.Pendiente)
            {
                Problem("Solo se pueden confirmar reservas que estén en estado 'pendiente'.");
            }
            else
            {
                booking.BookingState = Enums.BookingState.Confirmada;
                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();
                Problem("El estado del pedido ha sido cambiado a 'confirmada'.");
            }

            return RedirectToAction(nameof(Details), new { booking.Id });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Booking booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.BookingState == Enums.BookingState.Cancelada)
            {
                Problem("No se puede cancelar una reserva que esté en estado 'cancelada'.");
            }
            else
            {
                booking.BookingState = Enums.BookingState.Cancelada;
                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();
                Problem("El estado del pedido ha sido cambiado a 'cancelado'.");
            }

            return RedirectToAction(nameof(Details), new { booking.Id });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Bookings == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }

        public async Task<IActionResult> Pay(int id)
        {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            Booking booking = await _context.Bookings.FindAsync(id);

            PayViewModel model = new()
            {
                Booking = booking,
                BookingId = id,
                User = user,
                PaymentMethod = Enums.PaymentMethod.Efectivo,
                Date = DateTime.Now,
                Amount = booking.TotalPrice
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(PayViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userHelper.GetUserAsync(User.Identity.Name);
                if (user == null)
                {
                    return NotFound();
                }
                //si no se agrega hidden for SpaceId en la vista AddBooking, el SpaceId se pierde y llega nulo a este punto.
                Booking booking = await _context.Bookings.FindAsync(model.BookingId);
                if (booking == null)
                {
                    return NotFound();
                }
                try
                {
                    Pay pay = new()
                    {
                        Booking = booking,
                        User = user,
                        Date = model.Date,                        
                        PaymentMethod = model.PaymentMethod,
                        Amount = model.Amount

                    };
                    _context.Add(pay);
                    await _context.SaveChangesAsync();
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Bookings == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Bookings == null)
            {
                return Problem("Entity set 'DataContext.Bookings'  is null.");
            }
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return (_context.Bookings?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
