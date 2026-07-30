using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lab15.Models;
using lab15.Services;
using System.Diagnostics;

namespace lab15.Controllers
{
    public class HomeController : Controller
    {
        private readonly StudentPortalContext _context;
        private readonly IAlaaStampService _stampServiceA;
        private readonly IAlaaStampService _stampServiceB;

        public HomeController(
            StudentPortalContext context,
            IAlaaStampService stampServiceA,
            IAlaaStampService stampServiceB)
        {
            _context = context;
            _stampServiceA = stampServiceA;
            _stampServiceB = stampServiceB;
        }

        public async Task<IActionResult> Index()
        {
            var students = await _context.Students
                .OrderBy(s => s.FullName)
                .ToListAsync();

            ViewBag.BuildNumber = 177;
            ViewBag.Owner = _stampServiceA.Owner;
            ViewBag.StampA = _stampServiceA.Stamp;
            ViewBag.StampB = _stampServiceB.Stamp;

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