using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lab16.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lab16.Controllers
{
    public class StudentsController : Controller
    {
        private readonly StudentPortalContext _context;

        public StudentsController(StudentPortalContext context)
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

        public async Task<IActionResult> Details(int id)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student is null)
            {
                return NotFound();
            }

            return View(student);
        }

        public async Task<IActionResult> ByYear(int year)
        {
            var students = await _context.Students
                .Where(s => s.YearOfStudy == year)
                .OrderBy(s => s.FullName)
                .ToListAsync();

            ViewData["Year"] = year;

            return View(students);
        }

        public async Task<IActionResult> Honours(string band)
        {
            if (string.IsNullOrWhiteSpace(band))
            {
                return NotFound();
            }

            IQueryable<Student> query = _context.Students;

            if (string.Equals(band, "first", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.Gpa >= 3.5);
            }
            else if (string.Equals(band, "second", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.Gpa >= 3.0 && s.Gpa < 3.5);
            }
            else
            {
                query = query.Where(s => s.Gpa < 3.0);
            }

            var students = await query
                .OrderBy(s => s.FullName)
                .ToListAsync();

            ViewData["Band"] = band.ToLowerInvariant();

            return View(students);
        }

        [Route("students/search")]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            IQueryable<Student> query = _context.Students;

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(s => s.FullName.Contains(name));
            }

            var students = await query
                .OrderBy(s => s.FullName)
                .ToListAsync();

            ViewData["Name"] = name;

            return View(students);
        }

        // ============================================================
        // Part C — Top(int count): highest-GPA students, capped by the
        // route's chained :int:range(1, MAX_YEAR) constraint.
        // ============================================================
        public async Task<IActionResult> Top(int count)
        {
            var students = await _context.Students
                .OrderByDescending(s => s.Gpa)
                .Take(count)
                .ToListAsync();

            return View("Index", students);
        }

        // ============================================================
        // Part D — Intake(string code): all students, only reachable
        // through the route guarded by my IntakeCodeConstraint.
        // ============================================================
        public async Task<IActionResult> Intake(string code)
        {
            var students = await _context.Students
                .OrderBy(s => s.FullName)
                .ToListAsync();

            return View("Index", students);
        }

        // ============================================================
        // Part E — About: attribute-routed, own address, own first name.
        //
        // /Students/About (the conventional default-route shape) is a
        // 404 because [Route] attribute routing REPLACES the
        // conventional route for this action entirely — once an action
        // carries its own [Route], the default route no longer applies
        // to it at all; the ONLY address that reaches it is exactly
        // what the attribute states.
        //
        // minGpa belongs in the query string, not the path, because the
        // Block 5 test is: does this value identify WHICH resource, or
        // does it refine HOW you want it? The resource here is "the
        // student list" — minGpa doesn't name a different resource, it
        // filters the same one, exactly like ?name= does for Search.
        // ============================================================
        [Route("about/alaa")]
        public async Task<IActionResult> About([FromQuery] double? minGpa)
        {
            double threshold = minGpa ?? 3.5; // MIN_GPA (Lab ID 11)

            var students = await _context.Students
                .Where(s => s.Gpa >= threshold)
                .OrderBy(s => s.FullName)
                .ToListAsync();

            return View("Index", students);
        }
    }
}