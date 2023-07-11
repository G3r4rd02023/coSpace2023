using CoSpace.Data;
using CoSpace.Data.Entities;
using CoSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;

namespace CoSpace.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _context;

        public HomeController(ILogger<HomeController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Calendar()
        {
            //ViewData["events"] = new[]
            //{
            //    new Event { Id = 1, Title = "Video for Marisa", StartDate = "2020-11-14"},
            //    new Event { Id = 2, Title = "Preparation", StartDate = "2020-11-12"},
            //};

            // Obtener las reservas de la tabla "reservas" desde tu base de datos
            List<Booking> reservas = _context.Bookings.Include(r => r.Space).ToList();
            // Convertir las reservas en un formato adecuado para FullCalendar
            List<object> eventos = new List<object>();

            foreach (Booking reserva in reservas)
            {
                // Crear un objeto de evento con los campos requeridos por FullCalendar
                var evento = new
                {
                    id = reserva.Id,
                    title = reserva.Space!.Name,
                    start = reserva.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = reserva.EndDate.ToString("yyyy-MM-ddTHH:mm:ss")                   
                };

                eventos.Add(evento);
            }
            // Pasar los eventos a la vista "Calendar"
            ViewBag.Eventos = JsonConvert.SerializeObject(eventos);

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}