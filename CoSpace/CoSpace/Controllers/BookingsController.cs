using CoSpace.Data;
using CoSpace.Data.Entities;
using CoSpace.Helpers;
using CoSpace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace CoSpace.Controllers
{
    public class BookingsController : Controller
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IFlashMessage _flashMessage;

        public BookingsController(DataContext context, IUserHelper userHelper,IFlashMessage flashMessage)
        {
            _context = context;
            _userHelper = userHelper;
            _flashMessage = flashMessage;
        }

        public IActionResult Index()
        {
            List<Booking> availableBookings = GetAvailableBookings();

            return View(availableBookings);
        }

        private List<Booking> GetAvailableBookings()
        {
            DateTime currentDate = DateTime.Now;

            List<Booking> availableBookings;

            availableBookings = _context.Bookings
                .Include(b => b.User)
                .Where(b => b.EndDate > currentDate)
                .ToList();

            return availableBookings;
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Bookings == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b=>b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
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

            Booking? booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.BookingState != Enums.BookingState.Pendiente)
            {
                _flashMessage.Danger("Solo se pueden confirmar reservas que estén en estado 'pendiente'.");
            }
            else
            {
                booking.BookingState = Enums.BookingState.Confirmada;
                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();
                _flashMessage.Confirmation("El estado de la reserva ha sido cambiado a 'confirmada'.");
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Booking? booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.BookingState == Enums.BookingState.Cancelada)
            {
                _flashMessage.Danger("No se puede cancelar una reserva en estado 'cancelada'.");
            }
            else
            {
                booking.BookingState = Enums.BookingState.Cancelada;
                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();
                _flashMessage.Confirmation("El estado de la reserva ha sido cambiado a 'cancelada'.");
            }

            return RedirectToAction(nameof(Index));
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
                catch (Exception exception)
                {
                    _flashMessage.Danger(string.Empty, exception.Message);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }

        public async Task<IActionResult> Pay(int id)
        {
            User user = await _userHelper.GetUserAsync(User.Identity!.Name!);
            if (user == null)
            {
                return NotFound();
            }

            Booking? booking = await _context.Bookings.FindAsync(id);

            PayViewModel model = new()
            {
                Booking = booking,
                BookingId = id,
                User = user,
                PaymentMethod = Enums.PaymentMethod.Efectivo,
                Date = DateTime.Now,
                Amount = booking!.TotalPrice
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(PayViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userHelper.GetUserAsync(User.Identity!.Name!);
                if (user == null)
                {
                    return NotFound();
                }
                //si no se agrega hidden for SpaceId en la vista AddBooking, el SpaceId se pierde y llega nulo a este punto.
                Booking? booking = await _context.Bookings.FindAsync(model.BookingId);
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
            try
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                _flashMessage.Danger(string.Empty, "Registro eliminado exitosamente!.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exception)
            {
                _flashMessage.Danger(string.Empty, exception.Message);
            }

            return View(booking);
        }        
    }
}
