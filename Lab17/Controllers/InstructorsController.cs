// LAB 17 — Lab ID: 11 | MIN_GPA_EDIT = 2.3 | MAX_YEAR_EDIT = 4
//
// Note: this lab's actual Edit target is Instructor, not Student — the
// task text's wording (Gpa/YearOfStudy) assumed Student, but the
// provided Views/Instructors folder makes clear Instructor is the real
// entity. Part D's [Range] boundaries were adapted onto Instructor's
// one numeric field, YearsOfExperience — see the comment above the
// Instructor class in StudentPortalContext.cs for the full reasoning.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lab17.Models;
using System.Threading.Tasks;

namespace lab17.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly StudentPortalContext _context;

        public InstructorsController(StudentPortalContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var instructors = await _context.Instructors
                .OrderBy(i => i.FullName)
                .ToListAsync();

            return View(instructors);
        }

        // ============================================================
        // Part B — the GET half of Edit
        // ============================================================

        // Part B: this action loads the instructor from the database,
        // rather than rendering an empty form the way Create()'s GET
        // half does, because Edit's whole purpose is to show the
        // EXISTING values of a real row so they can be changed — an
        // empty form would have nothing to pre-fill and nothing to
        // compare a change against.
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.Id == id);

            if (instructor is null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // ============================================================
        // Part C — the POST half of Edit
        // ============================================================

        // Part C: if I renamed this parameter to instructorId while the
        // route pattern still said {id}, model binding would no longer
        // find a match by name — the route's {id} segment would never
        // populate instructorId, which would silently stay 0 (its
        // default value) instead of throwing any error at all.
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Instructor instructor)
        {
            // Guard clause FIRST, before any save.
            if (id != instructor.Id || !ModelState.IsValid)
            {
                return View(instructor);
            }

            _context.Instructors.Update(instructor);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{instructor.FullName} was updated";

            // Part E — redirect to the personally-named confirmation
            // action instead of Index.
            return RedirectToAction("AlaaConfirmed", new { id = instructor.Id });
        }

        // ============================================================
        // Part E — a confirmation action carrying my own first name
        // ============================================================

        // Part E: this action must reload the instructor from the
        // database rather than reusing the object the POST already had
        // in memory, because the POST's in-memory object only reflects
        // what was just submitted — reloading from the database proves
        // the change genuinely persisted, showing the row exactly as it
        // now stands in storage rather than trusting an object that was
        // never actually re-read after the save.
        public async Task<IActionResult> AlaaConfirmed(int id)
        {
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.Id == id);

            if (instructor is null)
            {
                return NotFound();
            }

            return View("Details", instructor);
        }
    }
}