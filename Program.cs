//B1: Prints 0, then False, then throws System.InvalidOperationException with the message
//"Sequence contains no elements." Count() and Any() are safe on empty collections — Count()
//returns 0, Any() returns False. But Average() has nothing to average and throws.

//B2: Prints "2 3 1 4" — the year keys in the order each first appears in the students list
//(Yara=2, Omar = 3, Nada = 1, Kareem = 4), NOT sorted numerically. GroupBy never sorts its groups;
//it buckets in first-encountered order.

//B3: Prints 3. The query is deferred — it doesn't run until Count() is called. By the time
//Count() executes, the new "Test Person"(year 3) has already been added, so it's included:
//Omar Hesham (3), Kareem Fouad (4), and Test Person (3) = 3 students with YearOfStudy >= 3.

// Lab ID: 11
// Part C threshold = 2.5 + ((11 mod 4) * 0.3) = 2.5 + (3 * 0.3) = 3.4
// Part D experibuild
// ence = (11 mod 5) + 3 = 1 + 3 = 4
// Part G property = 11 mod 3 = 2 -> DateTime EnrollmentDate

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace StudentPortalConsole
{
    public class Student
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public int YearOfStudy { get; set; }
        public double Gpa { get; set; }
        public DateTime EnrollmentDate { get; set; } // Part G property ( 11 mod 3 = 2)
    }

    public class Course
    {
        public int Id { get; set; }
        public string CourseName { get; set; } = "";
        public int Credits { get; set; }
    }

    public class Instructor
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public int YearsOfExperience { get; set; }
        public string? AssignedCourseName { get; set; }
    }

    // Part E.4 — custom extension method, top-level, non-generic, static
    public static class StudentQueryExtensions
    {
        public static IEnumerable<Student> MyTopStudents(this IEnumerable<Student> source)
        {
            // Threshold = 3.4 
            return source.Where(s => s.Gpa >= 3.4);
        }
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
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            List<Student> students = new List<Student>
            {
                new Student { FullName = "Yara Adel",    YearOfStudy = 2, Gpa = 3.5 },
                new Student { FullName = "Omar Hesham",  YearOfStudy = 3, Gpa = 2.8 },
                new Student { FullName = "Nada Samir",   YearOfStudy = 1, Gpa = 3.9 },
                new Student { FullName = "Kareem Fouad", YearOfStudy = 4, Gpa = 3.2 }
            };

            List<Instructor> instructors = new List<Instructor>
            {
                new Instructor { FullName = "Hamdy", YearsOfExperience = 10, AssignedCourseName = "Web Development Using .NET" },
                new Instructor { FullName = "Mona Khalil", YearsOfExperience = 6, AssignedCourseName = "Database Fundamentals" }
            };

            List<Course> courses = new List<Course>
            {
                new Course { CourseName = "Web Development Using .NET", Credits = 4 },
                new Course { CourseName = "Database Fundamentals", Credits = 3 }
            };

            Console.WriteLine("===== WARM-UP: Session 12's chain =====");
            var warmUp = students.Where(s => s.Gpa > 3.0).OrderByDescending(s => s.Gpa).Select(s => s.FullName).ToList();
            foreach (string n in warmUp) Console.WriteLine($"  {n}");

            
            
            
            Console.WriteLine();
            Console.WriteLine("===== Part C: Aggregates and Grouping =====");

            Console.WriteLine($"Total count: {students.Count()}");
            Console.WriteLine($"Above threshold (3.4): {students.Count(s => s.Gpa > 3.4)}");
            Console.WriteLine($"Average GPA: {students.Average(s => s.Gpa):F2}");
            Console.WriteLine($"Highest GPA: {students.Max(s => s.Gpa)}");
            Console.WriteLine($"Lowest GPA: {students.Min(s => s.Gpa)}");
            Console.WriteLine($"Any below 2.0: {students.Any(s => s.Gpa < 2.0)}");
            Console.WriteLine($"All at or above 2.0: {students.All(s => s.Gpa >= 2.0)}");

            // Trigger the exception deliberately
            List<Student> emptyList = new List<Student>();
            Console.WriteLine($"Empty Count(): {emptyList.Count()}");
            Console.WriteLine($"Empty Any(): {emptyList.Any()}");
            try
            {
                Console.WriteLine(emptyList.Average(s => s.Gpa));
            }
            catch (Exception ex)
            {
                // Exception recorded: System.InvalidOperationException
                // Message: "Sequence contains no elements."
                Console.WriteLine($"Caught: {ex.GetType().Name}: {ex.Message}");
            }

            // Fixed with a guard
            if (emptyList.Any())
            {
                Console.WriteLine(emptyList.Average(s => s.Gpa));
            }
            else
            {
                Console.WriteLine("Average skipped — collection is empty.");
            }

            // Group by year of study
            Console.WriteLine("-- Grouped by year --");
            foreach (var g in students.GroupBy(s => s.YearOfStudy))
            {
                Console.WriteLine($"Year {g.Key}: {g.Count()} student(s)");
                foreach (Student s in g) Console.WriteLine($"   {s.FullName}");
            }
            // The groups did NOT come out sorted — they appear in
            // first-encountered order (2, 3, 1, 4). GroupBy never sorts
            // its output; it only buckets by key in the order each key
            // is first seen while iterating the source.

            // Group by a computed key using the threshold (3.4)
            Console.WriteLine("-- Grouped by my own band (Achiever / Developing, threshold 3.4) --");
            foreach (var g in students.GroupBy(s => s.Gpa >= 3.4 ? "Achiever" : "Developing"))
            {
                Console.WriteLine($"{g.Key}: {g.Count()} student(s)");
                foreach (Student s in g) Console.WriteLine($"   {s.FullName} ({s.Gpa:F2})");
            }

            // Same grouping, sorted by key
            Console.WriteLine("-- Grouped by my own band, sorted by key --");
            foreach (var g in students.GroupBy(s => s.Gpa >= 3.4 ? "Achiever" : "Developing").OrderBy(g => g.Key))
            {
                Console.WriteLine($"{g.Key}: {g.Count()} student(s)");
            }
            // Operator added: OrderBy(g => g.Key)

            
            // Part D 
            
            Console.WriteLine();
            Console.WriteLine("===== Part D: Join =====");

            Console.WriteLine("-- Who teaches what (method syntax) --");
            var teaching = instructors.Join(courses,
                i => i.AssignedCourseName,
                c => c.CourseName,
                (i, c) => $"{i.FullName} teaches {c.CourseName} ({c.Credits} credits)");
            foreach (string line in teaching) Console.WriteLine($"   {line}");

            Console.WriteLine("-- Who teaches what (query syntax) --");
            var teachingQuery = from i in instructors
                                join c in courses on i.AssignedCourseName equals c.CourseName
                                select $"{i.FullName} teaches {c.CourseName} ({c.Credits} credits)";
            foreach (string line in teachingQuery) Console.WriteLine($"   {line}");

            // Add myself as an instructor — experience = 4, course doesn't exist
            Instructor me = new Instructor
            {
                FullName = "Alaa Hazem Helmy",
                YearsOfExperience = 4,
                AssignedCourseName = "Machine Learning"
            };
            instructors.Add(me);

            var teachingWithMe = instructors.Join(courses,
                i => i.AssignedCourseName,
                c => c.CourseName,
                (i, c) => $"{i.FullName} teaches {c.CourseName}");

            Console.WriteLine($"{instructors.Count} instructors in, {teachingWithMe.Count()} rows out.");
            // The numbers differ because Join uses INNER JOIN semantics:
            // an instructor whose AssignedCourseName matches no course in
            // the second collection simply produces no output row at all.
            // No exception is raised because nothing about this is
            // actually invalid — Join's contract is "match keys, skip
            // anything that doesn't match," not "every input must match."

            // To include my own row anyway, with a blank course, I would
            // need a LEFT (OUTER) JOIN — done in LINQ with
            // GroupJoin(...) combined with SelectMany(...) and
            // DefaultIfEmpty(), which supplies a null/default Course
            // for any instructor with no matching course.

            instructors.Remove(me);

            
            // Part E 
            
            Console.WriteLine();
            Console.WriteLine("===== Part E: Deferred Execution =====");

            // Prediction: this will print 4, since the query doesn't run
            // until Count() is called, by which point Layla has already
            // been added.
            var deferredQuery = students.Where(s => s.Gpa > 3.0);
            students.Add(new Student { FullName = "Layla Mostafa", YearOfStudy = 2, Gpa = 3.7 });
            Console.WriteLine($"Deferred count (includes Layla): {deferredQuery.Count()}");
            // Prediction was correct.
            students.RemoveAt(students.Count - 1);

            // Multiple enumeration bug — filters three separate times
            var highAchievers = students.Where(s => s.Gpa > 3.0);
            Console.WriteLine($"Count (run 1): {highAchievers.Count()}");
            foreach (Student s in highAchievers) Console.WriteLine($"   {s.FullName}"); // run 2
            Console.WriteLine($"Average (run 3): {highAchievers.Average(s => s.Gpa):F2}");
            // The filtering runs three separate times — once per
            // enumeration (Count, foreach, Average).

            // Fixed version
            var highAchieversList = students.Where(s => s.Gpa > 3.0).ToList();
            Console.WriteLine($"Fixed count: {highAchieversList.Count}");
            Console.WriteLine($"Fixed average: {highAchieversList.Average(s => s.Gpa):F2}");
            // What changed: ToList() forces the query to run exactly
            // once and store the results in memory; every read after
            // that just reads the list. This matters far more once the
            // same shape targets a database in Part H — three separate
            // enumerations there means three separate network round
            // trips to SQL Server, not just three harmless in-memory
            // loops.

            // Custom operator — threshold 3.4
            Console.WriteLine("-- My top students (custom extension method) --");
            var myTop = students.MyTopStudents().OrderBy(s => s.FullName).Select(s => s.FullName).ToList();
            foreach (string n in myTop) Console.WriteLine($"   {n}");
            // MyTopStudents() is deferred, not immediate — its body just
            // returns the result of Where(...), which itself is
            // deferred. Nothing about it forces enumeration; it only
            // actually runs once something consumes it, here via
            // .ToList() at the end of the chain.

            
            // Blocks 4-5 / Parts F-H — EF Core
           
            Console.WriteLine();
            Console.WriteLine("===== Parts F-H: EF Core =====");

            try
            {
                using (var context = new StudentPortalContext())
                {
                    if (!context.Students.Any())
                    {
                        context.Students.Add(new Student { FullName = "Yara Adel", YearOfStudy = 2, Gpa = 3.5, EnrollmentDate = DateTime.Now });
                        context.Students.Add(new Student { FullName = "Omar Hesham", YearOfStudy = 3, Gpa = 2.8, EnrollmentDate = DateTime.Now });
                        context.Students.Add(new Student { FullName = "Nada Samir", YearOfStudy = 1, Gpa = 3.9, EnrollmentDate = DateTime.Now });
                        context.Students.Add(new Student { FullName = "Kareem Fouad", YearOfStudy = 4, Gpa = 3.2, EnrollmentDate = DateTime.Now });
                        context.SaveChanges();
                        Console.WriteLine("Seeded 4 students into the database.");
                    }

                    var dbTopNames = context.Students
                        .Where(s => s.Gpa > 3.0)
                        .OrderByDescending(s => s.Gpa)
                        .Select(s => s.FullName)
                        .ToList();

                    Console.WriteLine("-- From the DATABASE --");
                    foreach (string n in dbTopNames) Console.WriteLine($"   {n}");

                    // Part H : the first line filters ON THE
                    // SERVER — EF translates .Where(...) into a SQL
                    // WHERE clause, so only matching rows ever cross
                    // the network. The second line pulls the ENTIRE
                    // table into memory with ToList() first, then
                    // filters in C# afterward — functionally the same
                    // result, but far more expensive on a real table.
                    // I used the first form (filter, then ToList()).

                    Console.WriteLine($"DB average GPA: {context.Students.Average(s => s.Gpa):F2}");
                    Console.WriteLine($"DB student count: {context.Students.Count()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("EF section could not run:");
                Console.WriteLine($"  {ex.GetType().Name}: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Done.");
        }
    }
}

// Part F.1: Which tables will Up() create?
// Three tables: Courses, Instructors, and Students.

// Part F.2: What column type did EF choose for Gpa, and for FullName?
// Gpa (a C# double) became SQL type "float". FullName (a C# string) became
// SQL type "nvarchar(max)" in all three tables.

// Part F.3: Is FullName nullable? Did I write anything that told EF that?
// No, FullName is NOT nullable (nullable: false) in all three tables.
// This came from initializing it as `public string FullName { get; set; } = "";`
// in each class — giving it a non-null default value told EF's nullable
// reference type analysis that this property should never be null, so it
// generated a NOT NULL column without me writing any explicit annotation.

// Part F.4: What would Down do if I ran it?
// Down() would drop all three tables — Courses, Instructors, and Students —
// completely removing them (and any data in them) from the database.


// Note: EnrollmentDate (Part G's property) was already added to the Student
// class before this migration was generated, so it appears directly in
// InitialCreate's Students table rather than in a separate follow-up
// migration. This means Part G's "Up operation" for my own property is
// technically part of this same CreateTable call rather than a standalone
// AddColumn operation — worth flagging honestly since it differs from the
// lab's expected two-step sequence.

// Part F: Two concrete schema differences between the EF-generated Students
// table and the Session 3 hand-built one:
//
// 1. Primary key naming and string type: EF named the primary key column
//    "Id" purely by convention (I never wrote anything telling it to), while
//    the hand-built table used an explicit "StudentID" name. Also, EF chose
//    nvarchar(max) for FullName by default, while the hand-built table used
//    a bounded varchar(100) — EF's default is far more permissive on
//    string length unless a max length is explicitly configured.
//
// 2. Nullability behavior: the hand-built table has a separate nullable
//    Email column with no EF equivalent at all (different columns
//    entirely), and every EF-generated column came out NOT NULL by
//    default unless the C# property type itself was nullable (e.g. a
//    string? or int?) — nullability in EF is driven directly by the C#
//    property's own type, not by a separate SQL-level decision made by
//    hand the way the original table's design was.




// Part I — Wrap-Up Reflection

// A silently missing join row is more dangerous to inherit than a crash because a crash is
// impossible to ignore — the program stops, the error is visible, and someone has to fix it
// before anything else can happen. A missing row from an INNER JOIN produces no error, no
// warning, nothing unusual at all — the program keeps running normally, and the only symptom
// is that some real data (my own instructor row, in Part D) simply never shows up anywhere.
// That kind of failure can sit undetected in a real system for a long time, silently producing
// incomplete reports or missing records, precisely because nothing about running the code looks
// wrong.

// Add-Migration and Update-Database being two separate commands is a safety feature, not an
// inconvenience, because it creates a mandatory checkpoint to actually read and understand what
// a schema change will do BEFORE it touches a real database. Add-Migration only generates a
// file describing the change — nothing in the database is touched yet. That gives a chance to
// open the migration, check exactly what Up will do, and catch a mistake while it's still just
// a file on disk. If both steps were combined into one command, a wrong migration would apply
// itself to the real database immediately, with no review step in between.

// Running the same LINQ chain against context.Students instead of a List<Student> looks
// identical in code, but the query itself is fundamentally different underneath: against a
// List, .Where(...) filters objects already sitting in memory; against a DbContext, EF
// translates that same .Where(...) into an actual SQL WHERE clause and sends it to SQL Server,
// which only returns the matching rows across the network. Deferred execution matters far more
// here because failing to force execution with .ToList() at the right moment (or enumerating
// the same query multiple times) doesn't just repeat a cheap in-memory loop — each enumeration
// becomes a separate real network round trip to the database, which is measurably slower and
// more expensive than anything Part E's in-memory version demonstrated.