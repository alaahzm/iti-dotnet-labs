// Part A: PreInit script reported 4 students already in the database,
// and 1 migration already applied (InitialCreate from Session 13).


//B1: After this runs, the database's Student with Id 1 will have Gpa = 3.99. Even though
//SaveChangesAsync is never called, the WriteLine still prints 3.99 because the change happened
//on the in-memory C# object right away — but nothing in the actual database changes, since
//without SaveChangesAsync, EF never sends any UPDATE statement to SQL Server at all.

//B2: Prints each instructor's name with Courses.Count = 0 for every one of them, even if they
//really do have courses in the database. Since Courses is initialized to new() but never
//populated via Include or explicit loading, and lazy loading is not enabled, the property just
//stays an empty list — no error, no warning, just silently wrong data.

//B3: The Gpa change is lost — nothing happens to the database, and no error is raised.
//AsNoTracking() tells EF not to track this entity at all, so when SaveChangesAsync runs, EF has
//no snapshot to compare against and doesn't know anything changed. This is dangerous because
//the code looks completely correct and runs without any exception, while silently doing nothing.


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;

// Lab ID: 11
// Part C GPA value = 3.0 + ((11 mod 7) * 0.1) = 3.0 + (4 * 0.1) = 3.4
// Part E delete behaviour = 11 mod 2 = 1 -> SetNull (InstructorId becomes int?)
// Part F extra course count = (11 mod 3) + 2 = 2 + 2 = 4

namespace StudentPortalConsole
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = "";

        public int YearOfStudy { get; set; }
        public double Gpa { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.Now; 
    }

    public class Course
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string CourseName { get; set; } = "";

        public int Credits { get; set; }

        // Lab ID 11 mod 2 = 1 -> SetNull behaviour requires a NULLABLE FK
        public int? InstructorId { get; set; }

        public Instructor? Instructor { get; set; }
    }

    public class Instructor
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = "";

        public int YearsOfExperience { get; set; }

        public List<Course> Courses { get; set; } = new();

        // AssignedCourseName deleted entirely — keeping both this and the
        // real FK relationship would mean two separate sources of truth
        // for "which course is this instructor teaching," and they could
        // drift out of sync with each other over time. The FK is the only
        // one the database can actually enforce, so it's the only one
        // that should exist.
    }

    public class StudentPortalContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Instructor> Instructors { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Server=.;Database=ITI_StudentPortalDB_EF;Trusted_Connection=True;TrustServerCertificate=True;");

            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fluent API — same FullName rule as the Data Annotation above.
            // If they ever disagreed, Fluent API wins over Data Annotations.
            modelBuilder.Entity<Student>()
                .Property(s => s.FullName)
                .IsRequired()
                .HasMaxLength(100);

            // Lab ID 11 mod 2 = 1 -> SetNull
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany(i => i.Courses)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }

    internal class Program
    {
        static async Task Main(string[] args)
        {
            using (var context = new StudentPortalContext())
            {
                Console.WriteLine("Students currently in the database:");
                foreach (var s in await context.Students.ToListAsync())
                {
                    Console.WriteLine($"  {s.FullName} — Year {s.YearOfStudy}, GPA {s.Gpa:F2}");
                }

                // Seed instructors, courses
                if (!await context.Instructors.AnyAsync())
                {
                    context.Instructors.Add(new Instructor { FullName = "Hamdy", YearsOfExperience = 10 });
                    context.Instructors.Add(new Instructor { FullName = "Mona Khalil", YearsOfExperience = 6 });
                    await context.SaveChangesAsync();
                    Console.WriteLine("Seeded 2 instructors: Hamdy and Mona Khalil.");
                }

                if (!await context.Courses.AnyAsync())
                {
                    context.Courses.Add(new Course { CourseName = "Web Development Using .NET", Credits = 4 });
                    context.Courses.Add(new Course { CourseName = "Database Fundamentals", Credits = 3 });
                    await context.SaveChangesAsync();
                    Console.WriteLine("Seeded 2 courses.");
                }
                // ============================================================
                // Part C — Full CRUD, Async, Verified. GPA value = 3.4
                // ============================================================
                Console.WriteLine();
                Console.WriteLine("===== Part C: CRUD =====");

                // C.1 — Read Nada Samir
                var nada = await context.Students.FirstAsync(s => s.FullName == "Nada Samir");
                Console.WriteLine($"C.1 Nada's current GPA: {nada.Gpa:F2}");

                // C.2 — Change but do NOT save yet
                nada.Gpa = 3.4;
                Console.WriteLine($"C.2 In C#, Nada's GPA is now: {nada.Gpa:F2}");
                // --> Check SSMS now. It will still show the OLD value,
                // because nothing has been sent to the database yet — the
                // change only exists on the in-memory tracked object until
                // SaveChangesAsync actually issues an UPDATE statement.

                // C.3 — Save, then re-check SSMS
                await context.SaveChangesAsync();
                Console.WriteLine($"C.3 After save, Nada's GPA in the database is now: {nada.Gpa:F2}");
                // EF knew to update ONLY Gpa (not FullName or YearOfStudy)
                // because it snapshotted every property's original value the
                // moment this entity was loaded. SaveChangesAsync compares
                // the current values against that snapshot and generates an
                // UPDATE statement listing only the columns that actually
                // differ — it never needs to be told explicitly.

                // C.4 — Create a new student (own name, year 2, GPA 3.4)
                var me = new Student { FullName = "Alaa Hazem Helmy", YearOfStudy = 2, Gpa = 3.4, EnrollmentDate = DateTime.Now };
                Console.WriteLine($"C.4 Id before save: {me.Id}"); // 0 — default int value
                await context.Students.AddAsync(me);
                await context.SaveChangesAsync();
                Console.WriteLine($"C.4 Id assigned by the database after save: {me.Id}");

                // C.5 — Update YearOfStudy to 3
                me.YearOfStudy = 3;
                await context.SaveChangesAsync();
                Console.WriteLine($"C.5 Updated YearOfStudy to: {me.YearOfStudy}");
                // --> Verify in SSMS: my row should now show YearOfStudy = 3

                // C.6 — Delete my own student
                context.Students.Remove(me);
                await context.SaveChangesAsync();
                Console.WriteLine("C.6 Deleted my own student row.");
                // Remove() has no async version because it does no actual
                // database work by itself — it only marks the tracked
                // entity's state as "Deleted" in memory. The real DELETE
                // statement (and everything else pending) is only issued
                // when SaveChangesAsync runs, as one single transaction.

                // ============================================================
                // Part D — Constraints
                // ============================================================
                Console.WriteLine();
                Console.WriteLine("===== Part D: Constraints =====");

                try
                {
                    var broken = new Student { FullName = null!, YearOfStudy = 2, Gpa = 3.0 };
                    await context.Students.AddAsync(broken);
                    await context.SaveChangesAsync();
                    Console.WriteLine("  Saved a NULL name — constraints are NOT applied yet.");
                }
                catch (DbUpdateException ex)
                {
                    // Exception type: Microsoft.EntityFrameworkCore.DbUpdateException
                    Console.WriteLine($"  Rejected a NULL FullName — [Required] enforced. Exception: {ex.GetType().Name}");
                    foreach (var entry in context.ChangeTracker.Entries<Student>()
                                                 .Where(e => e.State == EntityState.Added))
                    {
                        entry.State = EntityState.Detached;
                    }
                }

                // Part D.4 :
                // Up performs an AlterColumn operation on FullName (both Students and
                // Instructors tables), changing type from nvarchar(max) to nvarchar(100)
                // with maxLength: 100, and nullable: false (oldNullable was not shown
                // explicitly, but oldType was nvarchar(max) with no maxLength, meaning
                // the prior state had no length restriction at all).
                //
                // Two kinds of existing row could make this migration fail:
                // 1. A row where FullName is NULL
                // 2. A row where FullName is longer than 100 characters (since the
                //    column is shrinking from unlimited length to a 100-character cap)

                // Note: because this migration was generated after copying Session 13's
                // InitialCreate migration in as the baseline, EF captured all of today's
                // model changes (both Part D's constraints AND Part E's relationship) in
                // this single migration, rather than as two separate ones. Functionally
                // everything still applies correctly and in the right order — this is a
                // process quirk from the copy-forward fix, not a logic error.

                // Part D.5 (SSMS check before applying):
                // SELECT * FROM Students WHERE FullName IS NULL OR LEN(FullName) > 100;
                // SELECT * FROM Instructors WHERE FullName IS NULL OR LEN(FullName) > 100;
                // Result: 0 rows in both cases — safe to apply the migration.

                // ============================================================
                // Part E — Relationship. Delete behaviour = SetNull
                // ============================================================
                Console.WriteLine();
                Console.WriteLine("===== Part E: Relationship =====");

                var hamdy = await context.Instructors.FirstAsync(i => i.FullName == "Hamdy");
                var webCourse = await context.Courses
                    .FirstOrDefaultAsync(c => c.CourseName == "Web Development Using .NET");

                if (webCourse != null)
                {
                    webCourse.InstructorId = hamdy.Id;
                    await context.SaveChangesAsync();
                    Console.WriteLine($"  Linked '{webCourse.CourseName}' to {hamdy.FullName}.");
                }

                try
                {
                    var orphan = new Course
                    {
                        CourseName = "Machine Learning",
                        Credits = 3,
                        InstructorId = 9999
                    };
                    await context.Courses.AddAsync(orphan);
                    await context.SaveChangesAsync();
                    Console.WriteLine("  Orphan course saved — the FK is NOT in place.");
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"  Rejected InstructorId 9999 — FK constraint enforced. Exception: {ex.GetType().Name}");
                    // AssignedCourseName, being a plain string with no real
                    // constraint, would have accepted this exact same bad
                    // data with zero complaint — no exception, no rejection,
                    // just a course silently pointing at a course name that
                    // matches nothing real.
                    foreach (var entry in context.ChangeTracker.Entries<Course>()
                                                 .Where(e => e.State == EntityState.Added))
                    {
                        entry.State = EntityState.Detached;
                    }
                }

                // ============================================================
                // Part F — Loading Strategies and N+1. Extra courses = 4
                // ============================================================
                Console.WriteLine();
                Console.WriteLine("===== Part F: Loading =====");

                if (!await context.Courses.AnyAsync(c => c.CourseName == "Extra Course 1"))
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        context.Courses.Add(new Course
                        {
                            CourseName = $"Extra Course {i}",
                            Credits = 3,
                            InstructorId = hamdy.Id
                        });
                    }
                    await context.SaveChangesAsync();
                    Console.WriteLine("Added 4 extra courses, all assigned to Hamdy.");
                }

                Console.WriteLine("-- WITHOUT Include --");
                var bare = await context.Instructors.ToListAsync();
                foreach (var i in bare)
                {
                    Console.WriteLine($"  {i.FullName} ({i.Courses.Count} course(s))");
                }
                // Every count printed here is 0, regardless of how many
                // real courses each instructor actually has, because
                // nothing populated the Courses navigation property.
                // Only ONE query ran — the query that fetched the
                // instructors themselves.

                Console.WriteLine("-- WITH Include --");
                var withCourses = await context.Instructors
                    .Include(i => i.Courses)
                    .ToListAsync();
                foreach (var i in withCourses)
                {
                    Console.WriteLine($"  {i.FullName} ({i.Courses.Count} course(s))");
                    foreach (var c in i.Courses)
                    {
                        Console.WriteLine($"     {c.CourseName} — {c.Credits} credits");
                    }
                }
                // Still only ONE query ran — Include translates into a
                // single SQL query with a LEFT JOIN, not one query per
                // instructor. SQL Server returned more ROWS than there are
                // instructors, because a LEFT JOIN repeats each instructor
                // row once per matching course — an instructor with 4
                // courses appears in 4 result rows. EF recognizes the
                // repeated instructor Id across those rows and collapses
                // them back into ONE Instructor object with 4 Courses in
                // its list, rather than 4 separate duplicate objects.

                Console.WriteLine("-- Explicit loading --");
                var oneInstructor = await context.Instructors.FirstAsync();
                Console.WriteLine($"  Before explicit load: {oneInstructor.Courses.Count}");
                await context.Entry(oneInstructor).Collection(i => i.Courses).LoadAsync();
                Console.WriteLine($"  After explicit load: {oneInstructor.Courses.Count}");
                // Two queries total here: one for the instructor, one
                // deliberate second query for their courses — same total
                // query count as Include in this single-instructor case,
                // but the second query happens explicitly, at a moment I
                // chose in the code, rather than automatically.

                Console.WriteLine("-- AsNoTracking, then attempt to save a change --");
                var readOnlyStudents = await context.Students.AsNoTracking().ToListAsync();
                if (readOnlyStudents.Count > 0)
                {
                    readOnlyStudents[0].Gpa = 1.0;
                    await context.SaveChangesAsync();
                    Console.WriteLine("  Attempted to change a GPA on an AsNoTracking() entity and save.");
                }
                // Checking SSMS afterward shows NOTHING changed. Because
                // AsNoTracking() entities are never added to the change
                // tracker in the first place, EF has no snapshot to compare
                // against and doesn't even know a change was attempted —
                // SaveChangesAsync silently does nothing for this object.

                // Part F note: "WITHOUT Include" showed Hamdy with 1 course, not 0, because
                // EF's change tracker automatically "fixes up" navigation properties between
                // entities that are ALREADY tracked in memory from earlier in this same
                // context — hamdy and webCourse were both loaded and linked in Part E, so
                // EF wired that one relationship up for free. This is different from Include
                // actually querying the database; it only reflects what's already sitting
                // in the tracker, which is why it shows 1 (not all 5) and why relying on
                // this instead of Include would be fragile and misleading.
            }

            Console.WriteLine();
            Console.WriteLine("Done.");
        }
    }
}

// Part E: FK constraint name confirmed from the exception message:
// FK_Courses_Instructors_InstructorId