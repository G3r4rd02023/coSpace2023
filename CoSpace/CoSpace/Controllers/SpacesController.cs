﻿
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
    [Authorize(Roles = "Admin")]
    public class SpacesController : Controller
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IFlashMessage _flashMessage;

        public SpacesController(DataContext context, IUserHelper userHelper, IFlashMessage flashMessage)
        {
            _context = context;
            _userHelper = userHelper;
            _flashMessage = flashMessage;
        }


        public IActionResult Index()
        {
            List<Space> availableSpaces = GetAvailableSpaces();

            return View(availableSpaces);
        }

        private List<Space> GetAvailableSpaces()
        {
            DateTime currentDate = DateTime.Now;

            List<Space> availableSpaces;

            // Obtener las reservas que están activas en el momento actual
            List<int> activeBookingIds = _context.Bookings
                .Include(b => b.User)
                .Where(b => b.StartDate <= currentDate && b.EndDate > currentDate)
                .Select(b => b.Space!.Id)
                .ToList();

            // Obtener los espacios que no están asociados a reservas activas
            availableSpaces = _context.Spaces
                .Where(s => !activeBookingIds.Contains(s.Id))
                .ToList();

            return availableSpaces;
        }


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
                try
                {
                    _context.Add(space);
                    await _context.SaveChangesAsync();
                    _flashMessage.Confirmation(string.Empty, "Espacio de trabajo creado exitosamente!.");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException!.Message.Contains("duplicate"))
                    {
                        _flashMessage.Danger(string.Empty, "Ya existe un espacio de trabajo con el mismo nombre.");
                    }
                    else
                    {
                        _flashMessage.Danger(string.Empty, dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception exception)
                {
                    _flashMessage.Danger(string.Empty, exception.Message);
                }
            }
            return View(space);
        }        

        public async Task<IActionResult> AddBooking(int id)
        {
            User user = await _userHelper.GetUserAsync(User.Identity!.Name!);
            if (user == null)
            {
                return NotFound();
            }

            Space? space = await _context.Spaces.FindAsync(id);

            BookingViewModel model = new()
            {
                SpaceId = id,
                User = user,
                BookingState = Enums.BookingState.Pendiente,
                Space = space,
                TotalPrice = space!.Price
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBooking(BookingViewModel model)
        {
            bool isSpaceAvailable = IsSpaceAvailable(model.SpaceId, model.StartDate, model.EndDate);

            if (!isSpaceAvailable)
            {
                _flashMessage.Danger("El espacio seleccionado no está disponible en el horario especificado.");
				return RedirectToAction(nameof(Index));
				//return View(model);
            }

            if (ModelState.IsValid)
            {
                User user = await _userHelper.GetUserAsync(User.Identity!.Name!);
                if (user == null)
                {
                    return NotFound();
                }
                //si no se agrega hidden for SpaceId en la vista AddBooking, el SpaceId se pierde y llega nulo a este punto.
                Space? space = await _context.Spaces.FindAsync(model.SpaceId);
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
                    _flashMessage.Confirmation("Has reservado exitosamente,da click en la seccion de reservas para ver mas informacion. ");
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        private bool IsSpaceAvailable(int spaceId, DateTime startDate, DateTime endDate)
        {
            // Verificar si hay alguna reserva existente en el mismo espacio y horario
            bool isAvailable = !_context.Bookings.Any(b =>
                b.Space!.Id == spaceId &&
                ((b.StartDate >= startDate && b.StartDate < endDate) || (b.EndDate > startDate && b.EndDate <= endDate)));

            return isAvailable;
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
                    _flashMessage.Warning(string.Empty, "Espacio de trabajo actualizado exitosamente!.");
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException!.Message.Contains("duplicate"))
                    {
                        _flashMessage.Danger(string.Empty, "Ya existe un espacio de trabajo con el mismo nombre.");
                    }
                    else
                    {
                        _flashMessage.Danger(string.Empty, dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception exception)
                {
                    _flashMessage.Danger(string.Empty, exception.Message);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(space);
        }

        
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
            try
            {
                _context.Spaces.Remove(space);
                await _context.SaveChangesAsync();
                _flashMessage.Danger(string.Empty, "Espacio de trabajo eliminado exitosamente!.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exception)
            {
                _flashMessage.Danger(string.Empty, exception.Message);
            }
            return View(space);
        }

        
               
    }
}
