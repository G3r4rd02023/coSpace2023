using CoSpace.Data;
using CoSpace.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace CoSpace.Controllers
{
    public class PaysController : Controller
    {
        private readonly DataContext _context;
        private readonly IFlashMessage _flashMessage;

        public PaysController(DataContext context, IFlashMessage flashMessage)
        {
            _context = context;
            _flashMessage = flashMessage;
        }


        public async Task<IActionResult> Index()
        {
            return _context.Pays != null ?
                        View(await _context.Pays.Include(p => p.User).ToListAsync()) :
                        Problem("Entity set 'DataContext.Pays'  is null.");
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Pays == null)
            {
                return NotFound();
            }

            var pay = await _context.Pays
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pay == null)
            {
                return NotFound();
            }

            return View(pay);
        }



        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Pays == null)
            {
                return NotFound();
            }

            var pay = await _context.Pays.FindAsync(id);
            if (pay == null)
            {
                return NotFound();
            }
            return View(pay);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pay pay)
        {
            if (id != pay.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pay);
                    await _context.SaveChangesAsync();
                }
                catch (Exception exception)
                {
                    _flashMessage.Danger(string.Empty, exception.Message);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(pay);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Pays == null)
            {
                return NotFound();
            }

            var pay = await _context.Pays
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pay == null)
            {
                return NotFound();
            }
            try
            {
                _context.Pays.Remove(pay);
                await _context.SaveChangesAsync();
                _flashMessage.Danger(string.Empty, "Registro eliminado exitosamente!.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exception)
            {
                _flashMessage.Danger(string.Empty, exception.Message);
            }

            return View(pay);
        }


    }
}
