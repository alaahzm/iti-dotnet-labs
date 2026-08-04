using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace lab17.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = "";

        public int YearOfStudy { get; set; }
        public double Gpa { get; set; }
        public DateTime EnrollmentDate { get; set; }
    }

    public class Course
    {
        public int Id { get; set; }
        public string CourseName { get; set; } = "";
        public int Credits { get; set; }
        public int? InstructorId { get; set; }
        public Instructor? Instructor { get; set; }
    }

    // Part D: [Range] is a validation-only attribute, not a schema
    // attribute — it lives on the model purely to be checked by
    // ModelState.IsValid at request time; Entity Framework never
    // translates it into a column type, constraint, or migration. To
    // prove this without opening the database: change the boundary,
    // rebuild, and run the app with NO migration at all — if the new
    // boundary is enforced immediately with zero Add-Migration/
    // Update-Database step, that proves nothing was written to the
    // schema, unlike [Required]/[MaxLength], which genuinely were
    // migrated back in Session 14.
    //
    // Adaptation note: the lab task's Part D instructions describe
    // tightening [Range] on "Gpa" and "YearOfStudy" — properties that
    // exist on Student, not on Instructor, the actual entity this
    // lab's Edit form targets. Instructor has only one numeric field,
    // YearsOfExperience, so both of my Lab-ID-derived values (MIN_GPA_EDIT
    // = 2.3, rounded to 2, and MAX_YEAR_EDIT = 4) are applied to that one
    // field's lower and upper bound respectively.
    public class Instructor
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = "";

        [Range(2, 4, ErrorMessage = "Years of experience must be between 2 and 4 for this portal (Alaa's boundary).")]
        public int YearsOfExperience { get; set; }

        public List<Course> Courses { get; set; } = new();
    }

    public class StudentPortalContext : DbContext
    {
        public StudentPortalContext(DbContextOptions<StudentPortalContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Instructor> Instructors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>()
                .Property(s => s.FullName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany(i => i.Courses)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}