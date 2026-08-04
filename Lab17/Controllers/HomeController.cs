// =====================================================================
// HomeController — CARRIED FORWARD FROM SESSION 15 (Rule 39)
// ITI Summer Training | Web Development Using .NET | Morning Group
//
// Finished exactly as Session 15 left it: an injected context, an async
// Index that queries the roster, and the Privacy and Error actions that
// came with the template.
//
// Not one line changes today. This controller is the CONTROL GROUP for
// the whole session: at the end of the day it will still be answering
// the default route, unchanged, sitting next to a controller that
// answers four routes you designed yourselves. Compare the two URLs and
// you can see exactly what routing bought you.
// =====================================================================

using lab17.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lab17.Models;
using System.Diagnostics;

namespace lab17.Controllers
{
    public class HomeController : Controller
    {
        private readonly StudentPortalContext _context;

        public HomeController(StudentPortalContext context) // Constructor Injection
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var students = await _context.Students
                .OrderBy(s => s.FullName)
                .ToListAsync();

            return View(students);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
