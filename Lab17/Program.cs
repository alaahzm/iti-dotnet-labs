// =====================================================================
// StudentPortalWeb — SESSION PROJECT (Style Guide Rule 20/34/35/39/40)
// ITI Summer Training | Web Development Using .NET | Morning Group
// Session 17 — Controllers: IActionResult, Model Binding, Validation
//
// THIS IS THE SAME PROJECT AGAIN (Rule 40). Third session running on
// StudentPortalWeb. Session 15 wired up Dependency Injection, Session 16
// designed the URLs, and today we finally open up the thing all those
// URLs have been pointing at.
//
// ⚠️ NOTHING IN THIS FILE IS A TODO TODAY. Routing is finished. Every
//    route below is exactly what the room typed in Session 16 —
//    uncommented and working, because Rule 39 says already-taught code
//    ships as real running code, never as something to re-type.
//
//    (If your own copy from yesterday still has these five routes
//    commented out from the Block 5 payoff demo, that is why this file
//    looks different from yours. Yours ended mid-demonstration; this one
//    is put back together.)
//
// THIS PROJECT IS DAY-READY (Rule 39). Press F5 right now, before a
// single TODO is done, and every URL from yesterday still answers:
//   /                              the home page
//   /students                      the roster
//   /students/3                    one student
//   /students/year/2               one academic year
//   /students/honours/first        one honours band
//   /students/search?name=nada     the attribute-routed search
//
// TODAY'S TODOs LIVE IN TWO OTHER FILES, and the lecture visits them in
// this order:
//
//   Controllers/StudentsController.cs   TODO 1     Block 1 — one action,
//                                                  five different answers
//   Controllers/StudentsController.cs   TODO 2     Block 2 — where does a
//                                                  parameter come from?
//   Controllers/StudentsController.cs   TODO 3-4   Block 3 — the form
//   Models/StudentPortalContext.cs      TODO 5     Block 4 — the rules
//   Controllers/StudentsController.cs   TODO 6     Block 4 — checking them
//   Controllers/StudentsController.cs   TODO 7     Block 5 — saving, then
//                                                  redirecting
//
// ⚠️ TODAY CHANGES NO SCHEMA. Not one migration, from any project. The
//    Session 14 console project is still the migration owner. Today's new
//    annotations are VALIDATION-only attributes, which Entity Framework
//    does not map to columns — Block 4 explains the difference, and why
//    getting it wrong would silently demand a migration.
//
//    Today DOES add rows: the whole point is a form that inserts a real
//    student. Session17_PreInit.sql prints what is in the table before
//    you start, and carries a clearly-marked clean-up statement for
//    afterwards.
//
// For the full, correct, runnable version (do NOT peek until you've
// tried it yourself, or you're checking your own work), see:
// ../StudentPortalWeb_Complete/Program.cs
// =====================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using lab17.Constraints;
using lab17.Models;
using System;

namespace lab17
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // =========================================================
            // PHASE ONE — WHAT CAN THIS APP DO?
            // =========================================================
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            // Session 16 — your own route constraint, still registered.
            builder.Services.AddRouting(options =>
            {
                options.ConstraintMap.Add("honourBand", typeof(HonourBandConstraint));
            });

            // Session 15, plus the SQL logging added in Session 16 so the
            // console shows every query Entity Framework really sends.
            // Today that log is the proof for the session's biggest claim:
            // when validation rejects a form, NO INSERT appears here.
            builder.Services.AddDbContext<StudentPortalContext>(options =>
            {
                options.UseSqlServer("Server=.;Database=ITI_StudentPortalDB_EF;Trusted_Connection=True;TrustServerCertificate=True;")
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging();
            });

            var app = builder.Build();
            // ↑↑↑ THE DIVIDING LINE. Above: what exists. Below: what runs.

            // =========================================================
            // PHASE TWO — HOW IS A REQUEST HANDLED?
            // =========================================================
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Session 15's middleware. Session 16 used it to tell two
            // identical 404s apart. Today it does a third job: it shows
            // you that ONE click on a Save button produces TWO separate
            // requests, which is the whole subject of Block 5.
            app.Use(async (context, next) =>
            {
                Console.WriteLine($"[START] Request path : {context.Request.Path}");
                await next.Invoke();
                Console.WriteLine($"[END] Request path : {context.Request.Path}");
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            //app.UseAuthentication();
            app.UseAuthorization();

            // =========================================================
            // THE ROUTE TABLE — Session 16's work, finished and left
            // alone. Read it top to bottom: this is still the whole
            // public address surface of the application.
            // =========================================================
            app.MapControllerRoute(
                name: "studentsList",
                pattern: "students",
                defaults: new { controller = "Students", action = "Index" });

            app.MapControllerRoute(
                name: "studentsDetails",
                pattern: "students/{id:int}",
                defaults: new { controller = "Students", action = "Details" });

            app.MapControllerRoute(
                name: "studentsByYear",
                pattern: "students/year/{year:int:range(1,4)}",
                defaults: new { controller = "Students", action = "ByYear" });

            app.MapControllerRoute(
                name: "studentsHonours",
                pattern: "students/honours/{band:honourBand}",
                defaults: new { controller = "Students", action = "Honours" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}

#region 📋 Full TODO Checklist
// ---------------------------------------------------------------------
// Nothing in Program.cs today. Routing finished yesterday.
//
// Controllers/StudentsController.cs
//   TODO 1: One action that answers with four different kinds of result  [Block 1]
//   TODO 2: One action that proves where each parameter came from        [Block 2]
//   TODO 3: The empty form — the GET half of Create                      [Block 3]
//   TODO 4: The POST half of Create, and the attribute that marks it     [Block 3]
//   TODO 6: Refuse to save when the submitted data breaks the rules      [Block 4]
//   TODO 7: Save, then redirect instead of rendering                     [Block 5]
//
// Models/StudentPortalContext.cs
//   TODO 5: Add the validation rules the form will be checked against    [Block 4]
// ---------------------------------------------------------------------
#endregion
