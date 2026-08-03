namespace lab16.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseName { get; set; } = "";
        public int Credits { get; set; }
        public int? InstructorId { get; set; }
        public Instructor? Instructor { get; set; }
    }
}