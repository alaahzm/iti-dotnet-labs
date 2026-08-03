// LAB 16 — Lab ID: 11 | MAX_YEAR = 4 | MIN_GPA = 3.5 | INTAKE_CODE = itiC
//
// The default route sits at the bottom of the table, not the top,
// because routing walks the table top to bottom and stops at the
// FIRST match. The default route's pattern is broad enough to match
// almost anything ({controller}/{action}/{id?}), so if it sat above
// the more specific routes, it would swallow requests meant for them
// first, and the specific routes below it would never be reached.

using lab16.Constraints;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using lab16.Constraints;
using lab16.Models;

namespace lab16
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddRouting(options =>
            {
                options.ConstraintMap.Add("honourband", typeof(HonourBandConstraint));
                // Part D — registered under the nickname "intakecode"
                options.ConstraintMap.Add("intakecode", typeof(IntakeCodeConstraint));
            });

            builder.Services.AddDbContext<StudentPortalContext>(options =>
            {
                options.UseSqlServer("Server=.;Database=ITI_StudentPortalDB_EF;Trusted_Connection=True;TrustServerCertificate=True;");
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.Use(async (context, next) =>
            {
                Console.WriteLine($"[START] Request path : {context.Request.Path}");
                await next.Invoke();
                Console.WriteLine($"[END] Request path : {context.Request.Path}");
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();

            // =====================================================
            // ROUTE TABLE — order matters, specific routes first,
            // default route last.
            // =====================================================

            // Part B — is it acceptable for two URLs to reach the same
            // action? Yes, defensibly — a URL is just an address, not
            // the resource's identity; multiple valid names for the
            // same page (a short alias vs. a formal path) is a normal,
            // common pattern as long as both are intentional and
            // documented, not an accidental collision.
            app.MapControllerRoute(
                name: "roster",
                pattern: "roster",
                defaults: new { controller = "Students", action = "Index" });

            app.MapControllerRoute(
                name: "studentsList",
                pattern: "students",
                defaults: new { controller = "Students", action = "Index" });

            app.MapControllerRoute(
                name: "studentDetails",
                pattern: "students/{id:int}",
                defaults: new { controller = "Students", action = "Details" });

            app.MapControllerRoute(
                name: "studentsByYear",
                pattern: "students/year/{year:int:range(1,4)}",
                defaults: new { controller = "Students", action = "ByYear" });

            app.MapControllerRoute(
                name: "studentHonours",
                pattern: "students/honours/{band:honourband}",
                defaults: new { controller = "Students", action = "Honours" });

            // Part C — MAX_YEAR = 4. count must be a whole number AND
            // fall in the inclusive range 1 to 4.
            // My MAX_YEAR (4) IS accepted, because
            // range(1,4) is inclusive on both ends.
            app.MapControllerRoute(
                name: "studentsTop",
                pattern: "students/top/{count:int:range(1,4)}",
                defaults: new { controller = "Students", action = "Top" });

            // Part D — guarded by my own IntakeCodeConstraint, registered
            // above under the nickname "intakecode".
            app.MapControllerRoute(
                name: "studentsIntake",
                pattern: "students/intake/{code:intakecode}",
                defaults: new { controller = "Students", action = "Intake" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}