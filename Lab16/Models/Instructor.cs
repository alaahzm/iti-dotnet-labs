namespace lab16.Models
{
    public class Instructor
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public int YearsOfExperience { get; set; }
        public List<Course> Courses { get; set; } = new();
    }
}