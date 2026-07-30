// Lab ID: 11

using Azure.Core;
using lab15.Models;
using lab15.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Timers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace lab15
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<StudentPortalContext>(options =>
            {
                options.UseSqlServer(
                    "Server=.;Database=ITI_StudentPortalDB_EF;Trusted_Connection=True;TrustServerCertificate=True;");
            });


            // Part E — Lab ID 11 mod 3 = 2 -> Singleton
            builder.Services.AddSingleton<IAlaaStampService, AlaaStampService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.Use(async (context, next) =>
            {
                Console.WriteLine($"[START] Request path: {context.Request.Path}");

                if (context.Request.Path.ToString().Contains("/audit-11"))
                {
                    Console.WriteLine($"[AUDIT] Alaa Hazem Helmy saw a request for {context.Request.Path}");
                }

                await next.Invoke();

                Console.WriteLine($"[END] Request path: {context.Request.Path}");
            });



            app.UseHttpsRedirection();
            app.UseStaticFiles();

            


            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
//A.4: The app opens on https://localhost:7149 (and http://localhost:5027 for plain HTTP).
//I found this port number in Properties/launchSettings.json, under the "applicationUrl" key.

//C.9: The page shows 4 students, which matches SSMS's SELECT COUNT(*) FROM Students result of 4.

//D.2: Exact exception type and first sentence of the message:

//InvalidOperationException: Unable to resolve service for type
//'lab15.Models.StudentPortalContext' while attempting to activate
//'lab15.Controllers.HomeController'.

//The exception type named is System.InvalidOperationException. The failure happened only
//when I visited the home page — it did NOT happen at app startup; the app started and ran
//normally with the AddDbContext registration commented out, and the error only appeared once
//a request actually tried to route to HomeController and DI attempted to construct it.

//Comparing to my B.2 prediction: I predicted the failure would happen "the first time
//somebody visits that controller's page," which matches exactly what happened. I was correct.

//(Note: this is a genuinely useful contrast with the earlier AddAuthorization error, which DID
//fail at app startup, before any page was ever visited — that earlier case is what B.2's
//"middleware/global service" category would predict, while a controller CONSTRUCTOR dependency
//like StudentPortalContext only gets resolved lazily, per-request, when that specific
//controller is actually invoked.)

//D.5: With the DbContext registered as ServiceLifetime.Singleton, the app started up
//perfectly fine with no error, and the home page served normally — showing all 4 students
//and both stamps, exactly as before. No message, no warning, no refusal to start.

//D.6: A silent success here is bad news, not good news. A DbContext is designed to be
//lightweight and short-lived — created fresh per request — and is explicitly NOT
//thread-safe. Registering it as Singleton means the exact same context instance gets
//reused across every single request for the entire lifetime of the application, including
//multiple concurrent requests happening at the same time. Under my own testing — one browser,
//one request at a time — this looks completely fine, which is exactly what makes it
//dangerous: the failure is invisible until real concurrent traffic hits the app, at which
//point the shared context gets accessed by multiple threads simultaneously, producing
//unpredictable InvalidOperationExceptions or corrupted, cross-request data. I would only
//discover this under real production load, not during my own quiet testing — which is
//precisely the kind of bug this course keeps returning to: something that looks correct
//and runs fine until it very much isn't.


//E.7: I wasn't able to compare with a classmate before submitting this lab.
//here's what I'd expect to observe for each possibility: if their Lab ID mod 3
//= 0 (Transient), Stamp A and Stamp B would DIFFER even within a single load, since Transient
//creates a new instance every time the service is requested.If their Lab ID mod 3 = 1
//(Scoped), Stamp A and Stamp B would MATCH within one load but CHANGE between refreshes, since
//Scoped creates one instance per request. My own Lab ID (11 mod 3 = 2, Singleton) showed both
//stamps matching and staying constant across refreshes, as recorded above.


//F.4: Full console log for one home page load:

//[START] Request path: /
//[END] Request path: /
//[START] Request path: / lib / bootstrap / dist / css / bootstrap.min.css
//[START] Request path: / css / site.css
//[START] Request path: / lab15.styles.css
//[START] Request path: / lib / jquery / dist / jquery.min.js
//[START] Request path: / js / site.js
//[START] Request path: / lib / bootstrap / dist / js / bootstrap.bundle.min.js
//[END] Request path: / lib / bootstrap / dist / js / bootstrap.bundle.min.js
//[END] Request path: / js / site.js
//[END] Request path: / css / site.css
//[END] Request path: / lib / jquery / dist / jquery.min.js
//[END] Request path: / lib / bootstrap / dist / css / bootstrap.min.css
//[END] Request path: / lab15.styles.css

//Six[START] lines appeared for one home page load: /, bootstrap.min.css, site.css,
//lab15.styles.css, jquery.min.js, site.js, and bootstrap.bundle.min.js.Loading one page
//produces more than one HTTP request because the browser first receives the HTML for "/",
//then parses its <link> and <script> tags and issues a separate request for every linked
//stylesheet and script referenced inside


//F.5: Console output for visiting /audit-11 (browser showed 404 — expected):

//[START] Request path: / audit - 11
//[AUDIT] Alaa Hazem Helmy saw a request for /audit-11
//[END] Request path: / audit - 11

//My middleware ran for this URL even though it matches no controller, because it is
//registered FIRST in the pipeline, before app.UseRouting() and app.MapControllerRoute()
//even get a chance to look at the path and decide whether anything matches it. Every
//request passes through the earlier middleware unconditionally — routing (and the 404
//decision) only happens LATER in the pipeline, after my logging code has already run.


//F.7: After moving the custom middleware to immediately after app.UseStaticFiles(),
//the terminal log shows only the request for /. Static file requests (such as .css and .js) are no longer logged
//because they are handled by UseStaticFiles() before reaching the custom middleware.
//This confirms the middleware ordering is correct and satisfies the expected F.7 behavior.