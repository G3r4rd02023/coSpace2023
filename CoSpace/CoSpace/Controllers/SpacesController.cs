
using CoSpace.Data;
using CoSpace.Data.Entities;
using CoSpace.Helpers;
using CoSpace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoSpace.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SpacesController : Controller
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public SpacesController(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        // GET: Spaces
        public async Task<IActionResult> Index()
        {
            return View(await _context.Spaces.ToListAsync());
        }

        // GET: Spaces/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Spaces == null)
            {
                return NotFound();
            }

            var space = await _context.Spaces
                .FirstOrDefaultAsync(m => m.Id == id);
            if (space == null)
            {
                return NotFound();
            }

            return View(space);
        }

        
        public IActionResult Create()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Space space)
        {
            if (ModelState.IsValid)
            {
                _context.Add(space);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(space);
        }        

        public async Task<IActionResult> AddBooking(int id)
        {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            Space space = await _context.Spaces.FindAsync(id);

            BookingViewModel model = new()
            {
                SpaceId = id,
                User = user,
                BookingState = Enums.BookingState.Pendiente,
                Space = space,
                TotalPrice = space.Price
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBooking(BookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userHelper.GetUserAsync(User.Identity.Name);
                if (user == null)
                {
                    return NotFound();
                }
                //si no se agrega hidden for SpaceId en la vista AddBooking, el SpaceId se pierde y llega nulo a este punto.
                Space space = await _context.Spaces.FindAsync(model.SpaceId);
                if (space == null)
                {
                    return NotFound();
                }
                try
                {
                    Booking booking = new()
                    {
                        Space = space,
                        User = user,
                        EndDate = model.EndDate,
                        StartDate = model.StartDate,
                        BookingState = Enums.BookingState.Pendiente,
                        TotalPrice = space.Price
                        
                    };
                    _context.Add(booking);
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

        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Spaces == null)
            {
                return NotFound();
            }

            var space = await _context.Spaces.FindAsync(id);
            if (space == null)
            {
                return NotFound();
            }
            return View(space);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Space space)
        {
            if (id != space.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(space);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpaceExists(space.Id))
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
            return View(space);
        }

        // GET: Spaces/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Spaces == null)
            {
                return NotFound();
            }

            var space = await _context.Spaces
                .FirstOrDefaultAsync(m => m.Id == id);
            if (space == null)
            {
                return NotFound();
            }

            return View(space);
        }

        // POST: Spaces/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Spaces == null)
            {
                return Problem("Entity set 'DataContext.Spaces'  is null.");
            }
            var space = await _context.Spaces.FindAsync(id);
            if (space != null)
            {
                _context.Spaces.Remove(space);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SpaceExists(int id)
        {
            return (_context.Spaces?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
